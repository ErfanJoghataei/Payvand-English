using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.DAL.Entities
{
    public class QRCodeEntity
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        public int? ShortenedLinkId { get; set; }
        public ShortenedLink? ShortenedLink { get; set; }

        public string? Link { get; set; }

        public string QrImagePath { get; set; }
        public string? LogoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool ISDeleted { get; set; } = false;
        
        public string BackGroundColor {  get; set; }
        public string ForeGroundColor { get; set; }

        public int  Size { get; set; }


    }
}
