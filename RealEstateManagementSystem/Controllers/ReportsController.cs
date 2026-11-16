using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.ViewModels;

namespace RealEstateManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Agent")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            ILogger<ReportsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Reports
        public async Task<IActionResult> Index()
        {
            // Property Statistics
            var allProperties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .ToListAsync();

            var totalProperties = allProperties.Count;
            var approvedProperties = allProperties.Count(p => p.Status == "Approved");
            var pendingProperties = allProperties.Count(p => p.Status == "Pending");
            var rejectedProperties = allProperties.Count(p => p.Status == "Rejected");
            var forSaleProperties = allProperties.Count(p => p.ListingType == "Sale");
            var forRentProperties = allProperties.Count(p => p.ListingType == "Rent");
            var totalPortfolioValue = allProperties.Where(p => p.ListingType == "Sale").Sum(p => p.Price);
            var averagePropertyPrice = totalProperties > 0 ? allProperties.Average(p => p.Price) : 0m;

            // User Statistics
            var allUsers = await _context.Users.ToListAsync();
            var totalUsers = allUsers.Count;
            var activeUsersCount = allUsers.Count(u => u.IsActive);

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var agentUsers = await _userManager.GetUsersInRoleAsync("Agent");
            var sellerUsers = await _userManager.GetUsersInRoleAsync("Seller");
            var clientUsers = await _userManager.GetUsersInRoleAsync("Client");

            // Appointment Statistics
            var allAppointments = await _context.Appointments.ToListAsync();
            var totalAppointments = allAppointments.Count;
            var pendingAppointments = allAppointments.Count(a => a.Status == "Pending");
            var confirmedAppointments = allAppointments.Count(a => a.Status == "Confirmed");
            var completedAppointments = allAppointments.Count(a => a.Status == "Completed");
            var cancelledAppointments = allAppointments.Count(a => a.Status == "Cancelled");

            // Inquiry Statistics
            var allInquiries = await _context.Inquiries.ToListAsync();
            var totalInquiries = allInquiries.Count;
            var pendingInquiries = allInquiries.Count(i => i.Status == "Pending");
            var respondedInquiries = allInquiries.Count(i => i.Status == "Responded");

            // Property Type Breakdown
            var propertiesByType = allProperties
                .GroupBy(p => p.PropertyType)
                .ToDictionary(g => g.Key, g => g.Count());

            // Top Rated Properties
            var topRatedProperties = allProperties
                .Where(p => p.Reviews.Any())
                .OrderByDescending(p => p.Reviews.Average(r => r.Rating))
                .Take(5)
                .ToList();

            var model = new ReportsViewModel
            {
                TotalProperties = totalProperties,
                ApprovedProperties = approvedProperties,
                PendingProperties = pendingProperties,
                RejectedProperties = rejectedProperties,
                ForSaleProperties = forSaleProperties,
                ForRentProperties = forRentProperties,
                TotalPortfolioValue = totalPortfolioValue,
                AveragePropertyPrice = averagePropertyPrice,

                TotalUsers = totalUsers,
                AdminCount = adminUsers.Count,
                AgentCount = agentUsers.Count,
                SellerCount = sellerUsers.Count,
                ClientCount = clientUsers.Count,
                ActiveUsersCount = activeUsersCount,

                TotalAppointments = totalAppointments,
                PendingAppointments = pendingAppointments,
                ConfirmedAppointments = confirmedAppointments,
                CompletedAppointments = completedAppointments,
                CancelledAppointments = cancelledAppointments,

                TotalInquiries = totalInquiries,
                PendingInquiries = pendingInquiries,
                RespondedInquiries = respondedInquiries,

                PropertiesByType = propertiesByType,
                TopRatedProperties = topRatedProperties
            };

            return View(model);
        }
    }
}
