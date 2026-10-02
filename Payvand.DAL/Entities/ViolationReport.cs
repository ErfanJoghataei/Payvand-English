namespace Payvand.DAL.Entities
{
    public class ViolationReport
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public int? GuestSessionId { get; set; }
        public GuestSession? GuestSession { get; set; }
        public int? ShortenedLinkId { get; set; }
        public ShortenedLink? ShortenedLink { get; set; }
        public string ReportedUrl { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ReporterEmail { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsReviewed { get; set; }
        public bool IsDeleted { get; set; }
    }
}
