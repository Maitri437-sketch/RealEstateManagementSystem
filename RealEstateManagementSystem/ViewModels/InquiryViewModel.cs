using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.ViewModels
{
    public class InquiryViewModel
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public string? PropertyTitle { get; set; }
        public string? PropertyAddress { get; set; }
        public decimal PropertyPrice { get; set; }
        public string? PropertyImageUrl { get; set; }

        public string? ClientId { get; set; }
        public string? ClientName { get; set; }
        public string? ClientEmail { get; set; }
        public string? ClientPhone { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(150, ErrorMessage = "Subject cannot exceed 150 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Response { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}
