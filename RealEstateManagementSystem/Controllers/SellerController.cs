using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.ViewModels;
using System.Security.Claims;

namespace RealEstateManagementSystem.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<SellerController> _logger;

        public SellerController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            ILogger<SellerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Seller/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(sellerId))
            {
                return Challenge();
            }

            var sellerUser = await _userManager.FindByIdAsync(sellerId);

            var myProperties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Agent)
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var appointments = await _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.Client)
                .Include(a => a.Agent)
                .Where(a => a.Property != null && a.Property.SellerId == sellerId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var inquiries = await _context.Inquiries
                .Include(i => i.Property)
                .Include(i => i.Client)
                .Where(i => i.Property != null && i.Property.SellerId == sellerId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                UserRole = "Seller",
                UserName = sellerUser?.FullName ?? "Seller",
                MyPropertiesCount = myProperties.Count,
                ApprovedProperties = myProperties.Count(p => p.Status == "Approved"),
                PendingApprovals = myProperties.Count(p => p.Status == "Pending"),
                RejectedProperties = myProperties.Count(p => p.Status == "Rejected"),
                TotalAppointments = appointments.Count,
                TotalInquiries = inquiries.Count,
                RecentProperties = myProperties,
                RecentAppointments = appointments.Take(6).ToList(),
                RecentInquiries = inquiries.Take(6).ToList()
            };

            return View(model);
        }

        // GET: /Seller/Properties
        public async Task<IActionResult> Properties()
        {
            var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var properties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Agent)
                .Where(p => p.SellerId == sellerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(properties);
        }
    }
}
