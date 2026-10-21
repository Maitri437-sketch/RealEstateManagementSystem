using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.ViewModels;
using System.Security.Claims;

namespace RealEstateManagementSystem.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AppointmentController> _logger;

        public AppointmentController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            ILogger<AppointmentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Appointment
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            var query = _context.Appointments
                .Include(a => a.Property)
                    .ThenInclude(p => p!.Images)
                .Include(a => a.Client)
                .Include(a => a.Agent)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                // Admins see all appointments
            }
            else if (User.IsInRole("Agent"))
            {
                query = query.Where(a => a.AgentId == currentUserId || (a.Property != null && a.Property.AgentId == currentUserId));
            }
            else if (User.IsInRole("Seller"))
            {
                query = query.Where(a => a.Property != null && a.Property.SellerId == currentUserId);
            }
            else // Client
            {
                query = query.Where(a => a.ClientId == currentUserId);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // GET: /Appointment/Book?propertyId=5
        [HttpGet]
        public async Task<IActionResult> Book(int propertyId)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Agent)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId!);

            var mainImage = property.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                            ?? property.Images.FirstOrDefault()?.ImageUrl
                            ?? "https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=800&auto=format&fit=crop&q=80";

            var model = new AppointmentViewModel
            {
                PropertyId = property.Id,
                PropertyTitle = property.Title,
                PropertyAddress = $"{property.Address}, {property.City}, {property.State}",
                PropertyPrice = property.Price,
                PropertyImageUrl = mainImage,
                ClientId = currentUserId,
                ClientName = currentUser?.FullName,
                ClientEmail = currentUser?.Email,
                ClientPhone = currentUser?.PhoneNumber,
                AgentId = property.AgentId,
                AgentName = property.Agent?.FullName ?? property.Seller?.FullName ?? "Assigned Representative",
                AppointmentDate = DateTime.Today.AddDays(1).AddHours(10)
            };

            return View(model);
        }

        // POST: /Appointment/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(AppointmentViewModel model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            if (model.AppointmentDate <= DateTime.UtcNow)
            {
                ModelState.AddModelError("AppointmentDate", "Appointment date and time must be in the future.");
            }

            if (ModelState.IsValid)
            {
                var property = await _context.Properties.FindAsync(model.PropertyId);
                if (property == null)
                {
                    return NotFound();
                }

                var appointment = new Appointment
                {
                    PropertyId = model.PropertyId,
                    ClientId = currentUserId,
                    AgentId = property.AgentId,
                    AppointmentDate = model.AppointmentDate,
                    Status = "Pending",
                    Notes = model.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);

                // Notify Assigned Agent or Seller
                var recipientId = property.AgentId ?? property.SellerId;
                if (!string.IsNullOrEmpty(recipientId))
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = recipientId,
                        Title = "New Viewing Request",
                        Message = $"A viewing tour was requested for '{property.Title}' on {model.AppointmentDate:MMM dd, yyyy h:mm tt}.",
                        LinkUrl = "/Appointment",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your viewing tour has been scheduled! The agent will confirm shortly.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /Appointment/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Property)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isAgent = appointment.AgentId == currentUserId || (appointment.Property != null && appointment.Property.AgentId == currentUserId);
            var isSeller = appointment.Property != null && appointment.Property.SellerId == currentUserId;
            var isClient = appointment.ClientId == currentUserId;

            // Only client can cancel their own, while admin/agent/seller can change to any valid status
            if (isClient && status == "Cancelled")
            {
                appointment.Status = "Cancelled";
            }
            else if (isAdmin || isAgent || isSeller)
            {
                appointment.Status = status;
            }
            else
            {
                return Forbid();
            }

            // Notify Client of the update
            _context.Notifications.Add(new Notification
            {
                UserId = appointment.ClientId,
                Title = $"Tour Status: {status}",
                Message = $"Your scheduled tour for '{appointment.Property?.Title}' has been updated to {status}.",
                LinkUrl = "/Appointment",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Appointment status updated to '{status}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
