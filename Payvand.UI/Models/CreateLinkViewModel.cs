namespace Payvand.UI.Models
{
    public class CreateLinkViewModel
    {
        public string OriginalUrl { get; set; }

        public string ShortenedLink { get; set; }
        public int ShortenerLinkId { get; set; }

        public string? ErrorMessage { get; set; } = null;

        public bool LoginRequired { get; set; } = false;
    }
}
