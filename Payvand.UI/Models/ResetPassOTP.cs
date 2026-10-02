using System.ComponentModel.DataAnnotations;

namespace Payvand.UI.Models
{
    public class ResetPassOTP
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(6), MaxLength(6)]
        public string Code { get; set; }
    }
}
