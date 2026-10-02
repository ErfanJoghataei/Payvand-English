using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Payvand.BusinessLogic
{
    public class SafeBrowsingService
    {
        private readonly HttpClient httpClient;
        private readonly string apiKey;
        private readonly ILogger<SafeBrowsingService> logger;

        public SafeBrowsingService(HttpClient httpClient, IConfiguration configuration, ILogger<SafeBrowsingService> logger)
        {
            this.httpClient = httpClient;
            apiKey = configuration["GoogleSafeBrowsing:Key"] ?? string.Empty;
            this.logger = logger;
        }

        public async Task<bool> IsUrlSafe(string url)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.LogWarning("SafeBrowsing check was skipped because its API key is missing.");
                return true;
            }

            try
            {
                logger.LogInformation("Checking URL with Google SafeBrowsing. Url: {Url}", url);
                var endpoint = $"https://safebrowsing.googleapis.com/v5alpha1/urls:search?key={apiKey}";

                var payload = new
                {
                    client = new { clientId = "payvand", clientVersion = "1.0" },
                    threatInfo = new
                    {
                        threatTypes = new[] { "MALWARE", "SOCIAL_ENGINEERING", "UNWANTED_SOFTWARE", "POTENTIALLY_HARMFUL_APPLICATION" },
                        platformTypes = new[] { "ANY_PLATFORM" },
                        threatEntryTypes = new[] { "URL" },
                        threatEntries = new[] { new { url } }
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.PostAsync(endpoint, content);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "SafeBrowsing returned non-success status. Url: {Url}, StatusCode: {StatusCode}, Response: {Response}",
                        url,
                        response.StatusCode,
                        result);
                    return true;
                }

                var isSafe = string.IsNullOrWhiteSpace(result) || result == "{}";
                if (isSafe)
                {
                    logger.LogInformation("SafeBrowsing marked URL as safe. Url: {Url}", url);
                }
                else
                {
                    logger.LogWarning("SafeBrowsing found a threat. Url: {Url}, Response: {Response}", url, result);
                }

                return isSafe;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SafeBrowsing check failed. Url: {Url}", url);
                return true;
            }
        }
    }
}
