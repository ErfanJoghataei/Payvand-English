
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Payvand.DAL.AppDbContext;
using Payvand.DAL.Entities;
using System.Net;
using UAParser;


namespace Payvand.UI.Controllers
{
    [Route("")]
    public class LinkController : Controller
    {
        private readonly PayvandDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<LinkController> logger;

        public LinkController(PayvandDbContext context, IWebHostEnvironment environment, IHttpClientFactory httpClientFactory, ILogger<LinkController> logger)
        {
            this.context = context;
            this.environment = environment;
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }



        [HttpGet]
        [HttpHead]
        [Route("{shortcode:regex(^[[a-zA-Z0-9]]{{6}}$)}")]
        public async Task<IActionResult> RedirectToOriginal(string shortcode)
        {
            logger.LogInformation("Short link redirect requested. ShortCode: {ShortCode}", shortcode);
            var link = await context.ShortenedLinks
                .Include(c => c.ClickLogs)
                .FirstOrDefaultAsync(c => c.ShorteCode == shortcode && c.ExpiresAt > DateTime.UtcNow);

           

            if (link == null)
            {
                logger.LogWarning("Short link not found or expired. ShortCode: {ShortCode}", shortcode);
                Response.StatusCode = StatusCodes.Status404NotFound;
                return RedirectToAction("Page404", "ErorPages");
            }

            var ipAddress = GetClientIpAddress();
            var (country, city) = GetGeoLocation(ipAddress);

            if (!await IsDestinationAvailableAsync(link.OriginalUrl))
            {
                logger.LogWarning("Short link destination is unavailable. LinkId: {LinkId}, ShortCode: {ShortCode}, Url: {Url}", link.Id, shortcode, link.OriginalUrl);
                return RedirectToAction("NetworkProblem", "ErorPages");
            }


            var useragent = HttpContext.Request.Headers.UserAgent.ToString();
            var parser = Parser.GetDefault();
            var clientinfo = parser.Parse(useragent);
            var browsername = clientinfo.UA.Family;
            var os = clientinfo.OS.Family;
            DeviceType deviceType;

            var deviceFamily = clientinfo.Device.Family ?? string.Empty;
            if (deviceFamily.Contains("ipad", StringComparison.OrdinalIgnoreCase) ||
                deviceFamily.Contains("tablet", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = DeviceType.Tablet;
            }
            else if (deviceFamily.Equals("Other", StringComparison.OrdinalIgnoreCase))
            {
                deviceType = DeviceType.Desktop;
            }
            else
            {
                deviceType = DeviceType.Mobile;
            }

            var log = new ClickLog
            {
                LinkId = link.Id,
                IpAddress = ipAddress,
                UserAgent = useragent,
                Referre = HttpContext.Request.Headers["Referer"],
                Browser = browsername,
                Os = os,
                Country = country,
                City = city, 
                DeviceType = deviceType,
                ClickedAt = DateTime.UtcNow,
                UserId = link.UserId
            };
           link.ClickLogs.Add(log);

            await context.SaveChangesAsync();
            logger.LogInformation("Short link redirected and click logged. LinkId: {LinkId}, ShortCode: {ShortCode}, IpAddress: {IpAddress}, Browser: {Browser}, Os: {Os}, DeviceType: {DeviceType}", link.Id, shortcode, ipAddress, browsername, os, deviceType);

            return Redirect(link.OriginalUrl);
        }

        private async Task<bool> IsDestinationAvailableAsync(string originalUrl)
        {
            if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return true;
            }

            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
                headRequest.Headers.UserAgent.ParseAdd("Payvand-LinkChecker/1.0");
                using var headResponse = await client.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead);

                if (headResponse.StatusCode == HttpStatusCode.MethodNotAllowed ||
                    headResponse.StatusCode == HttpStatusCode.NotImplemented)
                {
                    return await CheckWithGetAsync(client, uri);
                }

                return (int)headResponse.StatusCode < 500;
            }
            catch (HttpRequestException)
            {
                logger.LogWarning("Destination HEAD check failed with HTTP error. Url: {Url}", originalUrl);
                return false;
            }
            catch (TaskCanceledException)
            {
                logger.LogWarning("Destination HEAD check timed out. Url: {Url}", originalUrl);
                return false;
            }
        }

        private static async Task<bool> CheckWithGetAsync(HttpClient client, Uri uri)
        {
            try
            {
                using var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                getRequest.Headers.UserAgent.ParseAdd("Payvand-LinkChecker/1.0");
                using var getResponse = await client.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead);
                return (int)getResponse.StatusCode < 500;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        private string GetClientIpAddress()
        {
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                return forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }

        private (string Country, string City) GetGeoLocation(string ipAddress)
        {
            const string unknown = "Unknown";
            if (!IPAddress.TryParse(ipAddress, out var parsedIp) ||
                IPAddress.IsLoopback(parsedIp) ||
                IsPrivateIp(parsedIp))
            {
                return (unknown, unknown);
            }

            var databasePath = Path.Combine(environment.ContentRootPath, "GeoLite2-City.mmdb");
            if (!System.IO.File.Exists(databasePath))
            {
                logger.LogWarning("GeoIP database file was not found. Path: {Path}", databasePath);
                return (unknown, unknown);
            }

            try
            {
                using var reader = new DatabaseReader(databasePath);
                var response = reader.City(parsedIp);
                return (response.Country.Name ?? unknown, response.City.Name ?? unknown);
            }
            catch (AddressNotFoundException)
            {
                logger.LogDebug("GeoIP address was not found. IpAddress: {IpAddress}", ipAddress);
                return (unknown, unknown);
            }
            catch (Exception)
            {
                logger.LogError("GeoIP lookup failed. IpAddress: {IpAddress}", ipAddress);
                return (unknown, unknown);
            }
        }

        private static bool IsPrivateIp(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                var v6Bytes = ipAddress.GetAddressBytes();
                return ipAddress.IsIPv6LinkLocal ||
                       ipAddress.IsIPv6SiteLocal ||
                       (v6Bytes[0] & 0xfe) == 0xfc;
            }

            var v4Bytes = ipAddress.GetAddressBytes();
            return v4Bytes[0] == 10 ||
                   v4Bytes[0] == 127 ||
                   (v4Bytes[0] == 172 && v4Bytes[1] >= 16 && v4Bytes[1] <= 31) ||
                   (v4Bytes[0] == 192 && v4Bytes[1] == 168);
        }
    }
}
