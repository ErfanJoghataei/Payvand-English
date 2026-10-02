namespace Payvand.UI.Models;

public sealed class LinkAnalyticsViewModel : LinkViewModel
{
    public string ShortUrl { get; init; } = string.Empty;
    public int TotalClicks { get; init; }
    public IReadOnlyList<LinkClickSummary> RecentClicks { get; init; } = [];
}

public sealed class LinkClickSummary
{
    public DateTime ClickedAt { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public string Device { get; init; } = string.Empty;
    public string? Browser { get; init; }
}
