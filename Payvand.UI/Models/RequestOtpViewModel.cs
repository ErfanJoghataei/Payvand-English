using System.ComponentModel.DataAnnotations;

namespace Payvand.UI.Models
{
    public class RequestOtpViewModel
    {
        [Required(ErrorMessage ="Enter your email.")]
        [EmailAddress(ErrorMessage ="Enter a valid email address.")]

        public string Email { get; set; }
    }
}
