using System.Text.Json.Serialization;

namespace Payvand.UI.Models;

public sealed class SeoMetadata
{
    public string Title { get; init; } = "Payvand | Free link shortener";
    public string Description { get; init; } = "Payvand helps you shorten links, create QR codes, and track clicks.";
    public string Keywords { get; init; } = "link shortener, short link, QR code, link analytics, Payvand";
    public string CanonicalPath { get; init; } = "/";
    public string ImagePath { get; init; } = "/Images/Logo/logo.png";
    public string Type { get; init; } = "website";
    public string Locale { get; init; } = "en_US";
    public bool NoIndex { get; init; }

    public string ToAbsoluteUrl(HttpRequest request, string pathOrUrl)
    {
        if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        var path = pathOrUrl.StartsWith('/') ? pathOrUrl : $"/{pathOrUrl}";
        return $"{request.Scheme}://{request.Host}{request.PathBase}{path}";
    }
}

public sealed class SeoSiteSchema
{
    [JsonPropertyName("@context")]
    public string Context => "https://schema.org";

    [JsonPropertyName("@type")]
    public string Type => "WebSite";

    public string Name => "Payvand";
    public string AlternateName => "Payvand";
    public string Url { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string InLanguage => "en-US";
}

public sealed class SeoArticleSchema
{
    [JsonPropertyName("@context")]
    public string Context => "https://schema.org";

    [JsonPropertyName("@type")]
    public string Type => "Article";

    [JsonPropertyName("headline")]
    public required string Headline { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("image")]
    public required string Image { get; init; }

    [JsonPropertyName("datePublished")]
    public DateTime DatePublished { get; init; }

    [JsonPropertyName("dateModified")]
    public DateTime DateModified { get; init; }

    [JsonIgnore]
    public string AuthorName { get; init; } = "Payvand Team";

    [JsonPropertyName("author")]
    public object Author => new Dictionary<string, object>
    {
        ["@type"] = "Organization",
        ["name"] = AuthorName
    };

    [JsonPropertyName("publisher")]
    public object Publisher => new Dictionary<string, object>
    {
        ["@type"] = "Organization",
        ["name"] = "Payvand",
        ["logo"] = new Dictionary<string, object>
        {
            ["@type"] = "ImageObject",
            ["url"] = Image
        }
    };
}
