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
    [Authorize(Roles = "Agent")]
    public class AgentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AgentController> _logger;

        public AgentController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            ILogger<AgentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Agent/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(agentId))
            {
                return Challenge();
            }

            var agentUser = await _userManager.FindByIdAsync(agentId);

            var assignedProperties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .Where(p => p.AgentId == agentId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var appointments = await _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.Client)
                .Where(a => a.AgentId == agentId || (a.Property != null && a.Property.AgentId == agentId))
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var inquiries = await _context.Inquiries
                .Include(i => i.Property)
                .Include(i => i.Client)
                .Where(i => i.Property != null && i.Property.AgentId == agentId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                UserRole = "Agent",
                UserName = agentUser?.FullName ?? "Agent",
                AssignedPropertiesCount = assignedProperties.Count,
                PendingAppointmentsCount = appointments.Count(a => a.Status == "Pending"),
                ConfirmedAppointmentsCount = appointments.Count(a => a.Status == "Confirmed"),
                TotalInquiries = inquiries.Count,
                RecentProperties = assignedProperties.Take(6).ToList(),
                RecentAppointments = appointments.Take(6).ToList(),
                RecentInquiries = inquiries.Take(6).ToList()
            };

            return View(model);
        }

        // POST: /Agent/UpdateAppointmentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, string status)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointment = await _context.Appointments
                .Include(a => a.Property)
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment != null && (appointment.AgentId == agentId || (appointment.Property != null && appointment.Property.AgentId == agentId)))
            {
                appointment.Status = status;

                // Send notification to the client
                _context.Notifications.Add(new Notification
                {
                    UserId = appointment.ClientId,
                    Title = $"Viewing Appointment {status}",
                    Message = $"Your appointment for '{appointment.Property?.Title}' on {appointment.AppointmentDate:MMM dd, yyyy h:mm tt} has been marked as {status}.",
                    LinkUrl = "/Appointment",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Appointment status updated to {status}.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        // POST: /Agent/RespondInquiry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondInquiry(int inquiryId, string response)
        {
            var agentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var inquiry = await _context.Inquiries
                .Include(i => i.Property)
                .FirstOrDefaultAsync(i => i.Id == inquiryId);

            if (inquiry != null && (inquiry.Property != null && inquiry.Property.AgentId == agentId))
            {
                inquiry.Response = response;
                inquiry.Status = "Responded";
                inquiry.RespondedAt = DateTime.UtcNow;

                // Notify client
                _context.Notifications.Add(new Notification
                {
                    UserId = inquiry.ClientId,
                    Title = "Inquiry Response Received",
                    Message = $"An agent responded to your inquiry about '{inquiry.Property.Title}'.",
                    LinkUrl = "/Inquiry",
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Response submitted to client.";
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
