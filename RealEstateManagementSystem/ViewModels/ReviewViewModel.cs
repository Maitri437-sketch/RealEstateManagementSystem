using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.ViewModels
{
    public class ReviewViewModel
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }

        public string? PropertyTitle { get; set; }

        [Required(ErrorMessage = "Please select a star rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Review comment is required")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
        public string Comment { get; set; } = string.Empty;

        public string? ClientName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
