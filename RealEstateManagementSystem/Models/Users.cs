using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.Models
{
    public class Users : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Property> PropertiesListed { get; set; } = new List<Property>();
        public virtual ICollection<Appointment> ClientAppointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Appointment> AgentAppointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<PropertyComparison> Comparisons { get; set; } = new List<PropertyComparison>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}