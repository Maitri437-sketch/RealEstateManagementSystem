using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.ViewModels;

namespace RealEstateManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalProperties = await _context.Properties.CountAsync();
            var pendingApprovals = await _context.Properties.CountAsync(p => p.Status == "Pending");
            var approvedProperties = await _context.Properties.CountAsync(p => p.Status == "Approved");
            var rejectedProperties = await _context.Properties.CountAsync(p => p.Status == "Rejected");
            var totalAppointments = await _context.Appointments.CountAsync();
            var totalInquiries = await _context.Inquiries.CountAsync();
            var totalReviews = await _context.Reviews.CountAsync();

            var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
            var agentUsers = await _userManager.GetUsersInRoleAsync("Agent");
            var sellerUsers = await _userManager.GetUsersInRoleAsync("Seller");
            var clientUsers = await _userManager.GetUsersInRoleAsync("Client");

            var recentProperties = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentAppointments = await _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.Client)
                .Include(a => a.Agent)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentInquiries = await _context.Inquiries
                .Include(i => i.Property)
                .Include(i => i.Client)
                .OrderByDescending(i => i.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                UserRole = "Admin",
                UserName = User.Identity?.Name ?? "Administrator",
                TotalUsers = totalUsers,
                TotalProperties = totalProperties,
                PendingApprovals = pendingApprovals,
                ApprovedProperties = approvedProperties,
                RejectedProperties = rejectedProperties,
                TotalAppointments = totalAppointments,
                TotalInquiries = totalInquiries,
                TotalReviews = totalReviews,
                AdminCount = adminUsers.Count,
                AgentCount = agentUsers.Count,
                SellerCount = sellerUsers.Count,
                ClientCount = clientUsers.Count,
                RecentProperties = recentProperties,
                RecentAppointments = recentAppointments,
                RecentInquiries = recentInquiries,
                RecentUsers = recentUsers
            };

            return View(model);
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var userViewModels = new List<Tuple<Users, string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault() ?? "None";
                userViewModels.Add(new Tuple<Users, string>(user, roleName));
            }

            ViewBag.UserList = userViewModels;
            return View();
        }

        // POST: /Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = $"User {user.FullName} status updated to {(user.IsActive ? "Active" : "Deactivated")}.";
            }
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/ChangeUserRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _roleManager.RoleExistsAsync(newRole))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);

                TempData["SuccessMessage"] = $"Role for {user.FullName} updated to {newRole}.";
            }
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/Properties
        public async Task<IActionResult> Properties(string? status)
        {
            var query = _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Agent)
                .Include(p => p.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
                ViewBag.CurrentStatus = status;
            }

            var properties = await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(properties);
        }

        // GET: /Admin/PendingApprovals
        public async Task<IActionResult> PendingApprovals()
        {
            var pendingProperties = await _context.Properties
                .Include(p => p.Seller)
                .Include(p => p.Images)
                .Where(p => p.Status == "Pending")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(pendingProperties);
        }

        // POST: /Admin/ApproveProperty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property != null)
            {
                property.Status = "Approved";
                property.UpdatedAt = DateTime.UtcNow;

                // Notify seller
                _context.Notifications.Add(new Notification
                {
                    UserId = property.SellerId,
                    Title = "Property Approved!",
                    Message = $"Your listing '{property.Title}' has been approved and is now live on the marketplace.",
                    LinkUrl = $"/Property/Details/{property.Id}",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Property '{property.Title}' has been approved and published.";
            }

            return RedirectToAction(nameof(PendingApprovals));
        }

        // POST: /Admin/RejectProperty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectProperty(int id, string? reason)
        {
            var property = await _context.Properties
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property != null)
            {
                property.Status = "Rejected";
                property.UpdatedAt = DateTime.UtcNow;

                // Notify seller
                _context.Notifications.Add(new Notification
                {
                    UserId = property.SellerId,
                    Title = "Listing Needs Revisions",
                    Message = $"Your listing '{property.Title}' was not approved. {(string.IsNullOrEmpty(reason) ? "Please review information and resubmit." : reason)}",
                    LinkUrl = $"/Property/Edit/{property.Id}",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = $"Property '{property.Title}' marked as Rejected.";
            }

            return RedirectToAction(nameof(PendingApprovals));
        }

        // POST: /Admin/ToggleFeatured
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                property.IsFeatured = !property.IsFeatured;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Property '{property.Title}' featured status updated.";
            }
            return RedirectToAction(nameof(Properties));
        }
    }
}
