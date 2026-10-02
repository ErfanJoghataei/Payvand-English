using System.ComponentModel.DataAnnotations;

namespace Payvand.UI.Models
{
    public class ResetPasswordViewModel
    {

        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 8,ErrorMessage ="Password must be at least eight characters.")]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword",ErrorMessage ="Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
