using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.ViewModels
{
    public class ReportsViewModel
    {
        // Property Statistics
        public int TotalProperties { get; set; }
        public int ApprovedProperties { get; set; }
        public int PendingProperties { get; set; }
        public int RejectedProperties { get; set; }
        public int ForSaleProperties { get; set; }
        public int ForRentProperties { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public decimal AveragePropertyPrice { get; set; }

        // User Statistics
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int AgentCount { get; set; }
        public int SellerCount { get; set; }
        public int ClientCount { get; set; }
        public int ActiveUsersCount { get; set; }

        // Appointment Statistics
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }

        // Inquiry Statistics
        public int TotalInquiries { get; set; }
        public int PendingInquiries { get; set; }
        public int RespondedInquiries { get; set; }

        // Breakdown by Property Type
        public Dictionary<string, int> PropertiesByType { get; set; } = new Dictionary<string, int>();

        // Top Rated Properties
        public List<Property> TopRatedProperties { get; set; } = new List<Property>();
    }
}
