using Payvand.DAL.Entities;

namespace Payvand.UI.Models
{
    public class CreatedQrCodes
    {
        public int Id { get; set; }
        public ShortenedLink shortenedLink { get; set; }
        public string link { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
