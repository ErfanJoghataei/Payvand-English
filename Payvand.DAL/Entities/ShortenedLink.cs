using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.DAL.Entities
{
    public class ShortenedLink
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public int? GuestSessionId { get; set; }
        public GuestSession GuestSession { get; set; }
        public string OriginalUrl { get; set; }
        public string Domain { get; set; }
        public ICollection<ClickLog> ClickLogs { get; set; } = new List<ClickLog>();
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsDeleted { get; set; }

        public string ShorteCode { get; set; }

        public QRCodeEntity QrCode { get; set; }

    }
}
