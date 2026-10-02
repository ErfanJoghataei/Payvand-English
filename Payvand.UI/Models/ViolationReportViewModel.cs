using System.ComponentModel.DataAnnotations;

namespace Payvand.UI.Models
{
    public class ViolationReportViewModel
    {
        [Required(ErrorMessage = "Enter the reported URL.")]
        public string ReportedUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Choose a reason for the report.")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the report details.")]
        [MinLength(10, ErrorMessage = "Details must be at least 10 characters.")]
        public string Description { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? ReporterEmail { get; set; }
    }
}
