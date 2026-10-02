using Payvand.DAL.Entities;

namespace Payvand.UI.Models
{
    public class QrCodeViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int Size { get; set; } = 300;
        public string QrImagePass { get; set; } = string.Empty;
        public IFormFile? Logo { get; set; }
        public string ForeGroundColor { get; set; } = "#000000";
        public string BackGroundColor { get; set; } = "#FFFFFF";
        public IEnumerable<ShortenedLink>? ShortenedLinks { get; set; }
        public int Count { get; set; }
        public ShortenedLink? SelectedShortenedLink { get; set; }
        public int? ShortenedLinkId { get; set; }
        public string? Link { get; set; }
        public List<QRCodeEntity> QrCodes { get; set; } = new List<QRCodeEntity>();
        public List<CreatedQrCodes> createdQrCodes { get; set; } = new List<CreatedQrCodes>();
    }
}
