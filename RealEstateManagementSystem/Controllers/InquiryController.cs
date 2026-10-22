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
    [Authorize]
    public class InquiryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<InquiryController> _logger;

        public InquiryController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            ILogger<InquiryController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Inquiry
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            var query = _context.Inquiries
                .Include(i => i.Property)
                    .ThenInclude(p => p!.Images)
                .Include(i => i.Client)
                .AsQueryable();

            if (User.IsInRole("Admin"))
            {
                // All inquiries
            }
            else if (User.IsInRole("Agent"))
            {
                query = query.Where(i => i.Property != null && i.Property.AgentId == currentUserId);
            }
            else if (User.IsInRole("Seller"))
            {
                query = query.Where(i => i.Property != null && i.Property.SellerId == currentUserId);
            }
            else // Client
            {
                query = query.Where(i => i.ClientId == currentUserId);
            }

            var inquiries = await query
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

            return View(inquiries);
        }

        // GET: /Inquiry/Create?propertyId=5
        [HttpGet]
        public async Task<IActionResult> Create(int propertyId)
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

            var model = new InquiryViewModel
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
                Subject = $"Inquiring about {property.Title}"
            };

            return View(model);
        }

        // POST: /Inquiry/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InquiryViewModel model)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            if (ModelState.IsValid)
            {
                var property = await _context.Properties.FindAsync(model.PropertyId);
                if (property == null)
                {
                    return NotFound();
                }

                var inquiry = new Inquiry
                {
                    PropertyId = model.PropertyId,
                    ClientId = currentUserId,
                    Subject = model.Subject.Trim(),
                    Message = model.Message.Trim(),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Inquiries.Add(inquiry);

                // Notify Agent or Seller
                var recipientId = property.AgentId ?? property.SellerId;
                if (!string.IsNullOrEmpty(recipientId))
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = recipientId,
                        Title = "New Property Question",
                        Message = $"A prospective buyer submitted an inquiry regarding '{property.Title}'.",
                        LinkUrl = "/Inquiry",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your question has been sent! A representative will respond to you shortly.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: /Inquiry/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int id, string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                TempData["ErrorMessage"] = "Response message cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var inquiry = await _context.Inquiries
                .Include(i => i.Property)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inquiry == null)
            {
                return NotFound();
            }

            inquiry.Response = response.Trim();
            inquiry.Status = "Responded";
            inquiry.RespondedAt = DateTime.UtcNow;

            // Notify Client
            _context.Notifications.Add(new Notification
            {
                UserId = inquiry.ClientId,
                Title = "Agent Replied to Your Inquiry",
                Message = $"An answer was posted regarding your inquiry on '{inquiry.Property?.Title}'.",
                LinkUrl = "/Inquiry",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Your response has been sent to the client.";
            return RedirectToAction(nameof(Index));
        }
    }
}
