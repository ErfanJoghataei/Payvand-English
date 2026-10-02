using System.ComponentModel.DataAnnotations;

namespace Payvand.UI.Models
{
    public class MessageViewModel
    {
        [Required(ErrorMessage = "Enter your full name.")]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Enter your email.")]
        [EmailAddress(ErrorMessage ="Enter a valid email address.")]
        [StringLength(254)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Enter your message.")]
        [StringLength(4000)]
        public string Message { get; set; }

        [Required(ErrorMessage = "Enter a subject.")]
        [StringLength(150)]
        public string Topic { get; set; }

        public int? UserId { get; set; }
        public int? GeustId { get; set; }



    }
}
