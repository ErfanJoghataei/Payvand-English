namespace Payvand.UI.Models
{
    public class ShortenLinkResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? ShortenedLink { get; set; }
        public int? ShortenerLinkId { get; set; }
        public bool LoginRequired { get; set; }
    }
}
