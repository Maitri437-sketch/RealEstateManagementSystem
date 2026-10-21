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
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<ClientController> _logger;

        public ClientController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            ILogger<ClientController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Client/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                return Challenge();
            }

            var clientUser = await _userManager.FindByIdAsync(clientId);

            var wishlists = await _context.Wishlists
                .Include(w => w.Property)
                    .ThenInclude(p => p!.Images)
                .Where(w => w.ClientId == clientId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            var appointments = await _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.Agent)
                .Where(a => a.ClientId == clientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var inquiries = await _context.Inquiries
                .Include(i => i.Property)
                .Where(i => i.ClientId == clientId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var comparisonsCount = await _context.PropertyComparisons
                .CountAsync(c => c.ClientId == clientId);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == clientId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                UserRole = "Client",
                UserName = clientUser?.FullName ?? "Client",
                WishlistCount = wishlists.Count,
                ComparisonsCount = comparisonsCount,
                TotalAppointments = appointments.Count,
                PendingAppointmentsCount = appointments.Count(a => a.Status == "Pending"),
                ConfirmedAppointmentsCount = appointments.Count(a => a.Status == "Confirmed"),
                TotalInquiries = inquiries.Count,
                UnreadNotificationsCount = notifications.Count(n => !n.IsRead),
                RecentAppointments = appointments.Take(5).ToList(),
                RecentInquiries = inquiries.Take(5).ToList(),
                Notifications = notifications,
                RecentProperties = wishlists.Select(w => w.Property!).Where(p => p != null).Take(4).ToList()
            };

            return View(model);
        }

        // POST: /Client/MarkNotificationRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == clientId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard));
        }

        // POST: /Client/MarkAllNotificationsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == clientId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
