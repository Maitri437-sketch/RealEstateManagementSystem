using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstateManagementSystem.Models
{
    public class Property
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Property title is required")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Property type is required")]
        [StringLength(50)]
        public string PropertyType { get; set; } = "House"; // Apartment, House, Villa, Condo, Commercial, Land

        [Required(ErrorMessage = "Listing type is required")]
        [StringLength(20)]
        public string ListingType { get; set; } = "Sale"; // Sale, Rent

        [Required(ErrorMessage = "Price is required")]
        [Range(0, 999999999.99, ErrorMessage = "Price must be a valid positive amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Zip code is required")]
        [StringLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Bedrooms must be 0 or more")]
        public int Bedrooms { get; set; } = 1;

        [Range(0, 100, ErrorMessage = "Bathrooms must be 0 or more")]
        public int Bathrooms { get; set; } = 1;

        [Range(0, 1000000, ErrorMessage = "Area must be a positive number")]
        public double AreaSqFt { get; set; }

        public int? YearBuilt { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Sold, Rented

        public bool IsFeatured { get; set; } = false;

        [Required]
        public string SellerId { get; set; } = string.Empty;

        [ForeignKey("SellerId")]
        public virtual Users? Seller { get; set; }

        public string? AgentId { get; set; }

        [ForeignKey("AgentId")]
        public virtual Users? Agent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<PropertyComparison> Comparisons { get; set; } = new List<PropertyComparison>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
