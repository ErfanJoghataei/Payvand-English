using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Payvand.UI.Models
{
    public class OtpLogin
    {

        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        [MinLength(6), MaxLength(6)]
        public string Code { get; set; }


        

      
    }
}
