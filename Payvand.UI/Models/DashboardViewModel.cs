using Payvand.DAL.Entities;

namespace Payvand.UI.Models
{
    public class DashboardViewModel
    {
        #region UserInfo
        public int UserId { get; set; }

        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string? DisplayName { get; set; }
        #endregion

        #region LinkTab
        public string OriginalUrl { get; set; }
        public string ShortenedLink { get; set; }

        public List<ShortenedLink> ShortenedLinks { get; set; } = new List<ShortenedLink>();
        public int LinkId { get; set; }

        #endregion

        #region UserSetting
        public string NewPassword { get; set; }
        public string NewPasswordSubmit { get; set; }
        #endregion

        #region BasicStats
        public int LinkCount { get; set; } = 0;

        public int TodayClicks { get; set; } = 0;

        public int AllClicks { get; set; } = 0;

        public List<ShortenedLink> LastLinks { get; set; } = new List<ShortenedLink>();




        #endregion

        #region QrCode

        public QrCodeViewModel Qrcdoe { get; set; } = new QrCodeViewModel();

        #endregion



    }
}
