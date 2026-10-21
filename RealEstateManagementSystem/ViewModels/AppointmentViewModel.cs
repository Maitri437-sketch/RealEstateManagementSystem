using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace RealEstateManagementSystem.ViewModels
{
    public class AppointmentViewModel
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

        [Display(Name = "Assigned Agent")]
        public string? AgentId { get; set; }
        public string? AgentName { get; set; }

        [Required(ErrorMessage = "Appointment date and time is required")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date & Time")]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1).AddHours(10);

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        [StringLength(500)]
        [Display(Name = "Notes / Message")]
        public string? Notes { get; set; }

        public IEnumerable<SelectListItem>? Agents { get; set; }
    }
}
