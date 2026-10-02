namespace Payvand.DAL.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? Password { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? GoogleSubject { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastLoginAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<ShortenedLink> ShortenedLinks { get; set; }

        public ICollection<QRCodeEntity> QRCodes { get; set; }
        public int LinkCount { get; set; }
        public DateTime? LastLinkQuotaResetAt { get; set; }
        public int QrCodeCount { get; set; }
        public DateTime? LastQrQuotaResetAt { get; set; }
    }
}
