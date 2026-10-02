using System.ComponentModel.DataAnnotations;

namespace Payvand.UI.Models
{
    public class PasswordLogin  
    {
        
        [Phone]
        [MinLength(11), MaxLength(11)]
        public string? PhoneNumber { get; set; }
         
        [Required(ErrorMessage ="Enter your password.")]
        [MinLength(8,ErrorMessage ="Password must be at least eight characters.")]
        
        public string Password { get; set; }

    }
}
