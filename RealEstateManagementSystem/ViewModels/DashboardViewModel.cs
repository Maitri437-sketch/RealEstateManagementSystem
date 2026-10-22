using RealEstateManagementSystem.Models;

namespace RealEstateManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public string UserRole { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        // Admin Statistics
        public int TotalUsers { get; set; }
        public int TotalProperties { get; set; }
        public int PendingApprovals { get; set; }
        public int ApprovedProperties { get; set; }
        public int RejectedProperties { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalInquiries { get; set; }
        public int TotalReviews { get; set; }

        public int AdminCount { get; set; }
        public int AgentCount { get; set; }
        public int SellerCount { get; set; }
        public int ClientCount { get; set; }

        // Agent / Seller / Client specific statistics
        public int AssignedPropertiesCount { get; set; }
        public int MyPropertiesCount { get; set; }
        public int PendingAppointmentsCount { get; set; }
        public int ConfirmedAppointmentsCount { get; set; }
        public int WishlistCount { get; set; }
        public int ComparisonsCount { get; set; }
        public int UnreadNotificationsCount { get; set; }

        // Recent Activity Lists
        public List<Property> RecentProperties { get; set; } = new List<Property>();
        public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
        public List<Inquiry> RecentInquiries { get; set; } = new List<Inquiry>();
        public List<Users> RecentUsers { get; set; } = new List<Users>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
