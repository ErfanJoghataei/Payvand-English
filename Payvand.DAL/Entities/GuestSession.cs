using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.DAL.Entities
{
    public class GuestSession
    {
        public int Id { get; set; }
        public string? SessionTokenHash { get; set; }
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public string? Referrer { get; set; }
        public DateTime FirstSeenAt { get; set; }
        public DateTime LastSeenAt { get; set; }

     
        public DateTime? ConvertedAt { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }
        public string? MetaData { get; set; }

        public ICollection<ShortenedLink> ShortenedLinks { get; set; }

        public int LinkCount { get; set; } = 0;
        public DateTime? LastResetAt { get; set; }
        public int QrCodeCount { get; set; }
        public DateTime? LastQrQuotaResetAt { get; set; }

        public void Increment()
        {
            ResetIfNeded();

            LinkCount++;
        }

        private void ResetIfNeded()
        {
            var now = DateTime.UtcNow;
             
            if(LastResetAt == null || now.Month != LastResetAt.Value.Month || now.Year != LastResetAt.Value.Year)
            {
                LinkCount = 0;
                LastResetAt = now;
            }
        }
    }
}
