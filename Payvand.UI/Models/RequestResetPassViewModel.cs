using System.ComponentModel.DataAnnotations;

namespace Payvand.UI.Models
{
    public class RequestResetPassViewModel
    {
        [Required(ErrorMessage = "Enter your mobile number.")]
        [MinLength(11, ErrorMessage = "Enter a valid mobile number."), MaxLength(11, ErrorMessage = "Enter a valid mobile number.")]
        public string PhoneNumber { get; set; }
    }
}
