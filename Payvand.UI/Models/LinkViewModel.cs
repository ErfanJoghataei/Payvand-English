namespace Payvand.UI.Models
{
    public class LinkViewModel
    {
        public string OriginalUrl { get; set; }

        public string ShortenedLink { get; set; }
        public int ShortenerLinkId { get; set; }

        public string? ErrorMessage { get; set; } = null;

        public bool LoginRequired { get; set; } = false;

        public MessageViewModel Messagemodel { get; set; } = new();
        public ViolationReportViewModel ViolationReport { get; set; } = new();


        public int TodayClicksCount { get; set; }
        public int LinksCount { get; set; }
        public int UsersCount { get; set; }

    }
}
