using System;
using System.Collections.Generic;
using System.Text;

namespace Payvand.DAL.Entities
{

    public class ClickLog
    {
        public int Id { get; set; }
        public int LinkId { get; set; }
        public ShortenedLink Link { get; set; }
        public int? UserId { get; set; }
        public User? User { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Referre { get; set; }
        public string? Country { get; set; }
       
        public string? City { get; set; }

        public DeviceType DeviceType { get; set; }
        public string? Browser { get; set; }
        public string? Os { get; set; }
        public DateTime ClickedAt { get; set; }


    }
    public enum DeviceType
    {
        Mobile,
        Desktop,
        Tablet
    }
}
