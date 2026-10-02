using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

const int virtualUsers = 100;
var duration = TimeSpan.FromSeconds(args.Length > 1 && int.TryParse(args[1], out var seconds) ? seconds : 30);
var baseUri = new Uri(args.Length > 0 ? args[0].TrimEnd('/') + "/" : "https://localhost:7147/");
var runId = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture);

Console.WriteLine($"Payvand load test | target={baseUri} | users={virtualUsers} | browse={duration.TotalSeconds:0}s");

var users = Enumerable.Range(0, virtualUsers)
    .Select(index => new VirtualUser(index, baseUri))
    .ToArray();

var arrival = new Metrics("fresh-arrival");
var arrivalGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var arrivalWallClock = Stopwatch.StartNew();
var arrivalTasks = users.Select(async user =>
{
    await arrivalGate.Task;
    var result = await user.GetAsync("", arrival);
    user.HomeHtml = result.Body;
    user.AntiForgeryToken = ExtractAntiForgeryToken(result.Body);
}).ToArray();
arrivalGate.SetResult();
await Task.WhenAll(arrivalTasks);
arrivalWallClock.Stop();
arrival.Print(arrivalWallClock.Elapsed);

var browse = new Metrics("steady-browse");
var browsePaths = new[]
{
    "",
    "blog",
    "blog/what-is-short-link",
    "blog/qr-code-complete-guide",
    "blog/analyze-link-clicks",
    "Dashboard/Dash"
};
var browseGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var browseWallClock = Stopwatch.StartNew();
var browseTasks = users.Select(async user =>
{
    await browseGate.Task;
    var random = new Random(unchecked(Environment.TickCount * 31 + user.Id));
    var deadline = Stopwatch.GetTimestamp() + (long)(duration.TotalSeconds * Stopwatch.Frequency);
    var pathIndex = user.Id % browsePaths.Length;

    while (Stopwatch.GetTimestamp() < deadline)
    {
        await user.GetAsync(browsePaths[pathIndex++ % browsePaths.Length], browse);
        await Task.Delay(random.Next(120, 321));
    }
}).ToArray();
browseGate.SetResult();
await Task.WhenAll(browseTasks);
browseWallClock.Stop();
browse.Print(browseWallClock.Elapsed);

var creation = new Metrics("create-link-burst");
var creationGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
var businessSuccesses = 0;
var missingTokens = 0;
var creationWallClock = Stopwatch.StartNew();
var creationTasks = users.Select(async user =>
{
    await creationGate.Task;
    if (string.IsNullOrWhiteSpace(user.AntiForgeryToken))
    {
        Interlocked.Increment(ref missingTokens);
        return;
    }

    var fields = new Dictionary<string, string>
    {
        ["OriginalUrl"] = $"https://example.com/payvand-benchmark/{runId}-{user.Id}",
        ["__RequestVerificationToken"] = user.AntiForgeryToken
    };
    var response = await user.PostFormAsync("Home/LinkShortener", fields, creation);
    if (response.StatusCode == HttpStatusCode.OK && ResponseWasSuccessful(response.Body))
    {
        Interlocked.Increment(ref businessSuccesses);
    }
}).ToArray();
creationGate.SetResult();
await Task.WhenAll(creationTasks);
creationWallClock.Stop();
creation.Print(creationWallClock.Elapsed);
Console.WriteLine($"  businessSuccesses={businessSuccesses} missingAntiForgeryTokens={missingTokens}");

foreach (var user in users)
{
    user.Dispose();
}

static string? ExtractAntiForgeryToken(string? html)
{
    if (string.IsNullOrWhiteSpace(html)) return null;
    var match = Regex.Match(
        html,
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"|value=\"([^\"]+)\"[^>]*name=\"__RequestVerificationToken\"",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    var encoded = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
    return string.IsNullOrWhiteSpace(encoded) ? null : WebUtility.HtmlDecode(encoded);
}

static bool ResponseWasSuccessful(string? body)
{
    if (string.IsNullOrWhiteSpace(body)) return false;
    try
    {
        using var json = JsonDocument.Parse(body);
        return json.RootElement.TryGetProperty("success", out var success) && success.GetBoolean();
    }
    catch (JsonException)
    {
        return false;
    }
}

sealed class VirtualUser : IDisposable
{
    private readonly HttpClient client;

    public VirtualUser(int id, Uri baseUri)
    {
        Id = id;
        var handler = new SocketsHttpHandler
        {
            CookieContainer = new CookieContainer(),
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.All,
            MaxConnectionsPerServer = 8,
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            SslOptions = { RemoteCertificateValidationCallback = (_, _, _, _) => true }
        };
        client = new HttpClient(handler)
        {
            BaseAddress = baseUri,
            Timeout = TimeSpan.FromSeconds(30)
        };
        client.DefaultRequestHeaders.UserAgent.ParseAdd($"PayvandLoadTest/1.0 VU-{id}");
    }

    public int Id { get; }
    public string? HomeHtml { get; set; }
    public string? AntiForgeryToken { get; set; }

    public async Task<ResponseResult> GetAsync(string path, Metrics metrics) =>
        await SendAsync(new HttpRequestMessage(HttpMethod.Get, path), metrics);

    public async Task<ResponseResult> PostFormAsync(string path, IReadOnlyDictionary<string, string> fields, Metrics metrics)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(fields)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        return await SendAsync(request, metrics);
    }

    private async Task<ResponseResult> SendAsync(HttpRequestMessage request, Metrics metrics)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            using (request)
            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
            {
                var body = await response.Content.ReadAsStringAsync();
                metrics.Record(response.StatusCode, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
                return new ResponseResult(response.StatusCode, body);
            }
        }
        catch (Exception exception)
        {
            metrics.RecordException(exception, Stopwatch.GetElapsedTime(started).TotalMilliseconds);
            return new ResponseResult(0, null);
        }
    }

    public void Dispose() => client.Dispose();
}

sealed record ResponseResult(HttpStatusCode StatusCode, string? Body);

sealed class Metrics(string name)
{
    private readonly ConcurrentBag<double> latencies = [];
    private readonly ConcurrentDictionary<int, int> statuses = new();
    private readonly ConcurrentDictionary<string, int> exceptions = new();

    public void Record(HttpStatusCode status, double elapsedMilliseconds)
    {
        latencies.Add(elapsedMilliseconds);
        statuses.AddOrUpdate((int)status, 1, (_, count) => count + 1);
    }

    public void RecordException(Exception exception, double elapsedMilliseconds)
    {
        latencies.Add(elapsedMilliseconds);
        exceptions.AddOrUpdate(exception.GetType().Name, 1, (_, count) => count + 1);
    }

    public void Print(TimeSpan wallClock)
    {
        var values = latencies.Order().ToArray();
        var successful = statuses.Where(pair => pair.Key is >= 200 and < 400).Sum(pair => pair.Value);
        var failed = values.Length - successful;
        Console.WriteLine();
        Console.WriteLine($"[{name}]");
        Console.WriteLine($"  requests={values.Length} success={successful} failed={failed} rps={(values.Length / wallClock.TotalSeconds):F2} wall={wallClock.TotalSeconds:F2}s");
        Console.WriteLine($"  latency ms: avg={Average(values):F2} p50={Percentile(values, .50):F2} p90={Percentile(values, .90):F2} p95={Percentile(values, .95):F2} p99={Percentile(values, .99):F2} max={(values.Length == 0 ? 0 : values[^1]):F2}");
        Console.WriteLine($"  status: {string.Join(", ", statuses.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}={pair.Value}"))}");
        if (!exceptions.IsEmpty)
        {
            Console.WriteLine($"  exceptions: {string.Join(", ", exceptions.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}={pair.Value}"))}");
        }
    }

    private static double Average(double[] values) => values.Length == 0 ? 0 : values.Average();

    private static double Percentile(double[] values, double percentile)
    {
        if (values.Length == 0) return 0;
        var index = (int)Math.Ceiling(percentile * values.Length) - 1;
        return values[Math.Clamp(index, 0, values.Length - 1)];
    }
}
