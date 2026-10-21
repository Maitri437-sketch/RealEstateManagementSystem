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
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            ILogger<ReviewController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Review
        public async Task<IActionResult> Index()
        {
            var query = _context.Reviews
                .Include(r => r.Property)
                .Include(r => r.Client)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                query = query.Where(r => r.ClientId == currentUserId || (r.Property != null && r.Property.SellerId == currentUserId));
            }

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        // POST: /Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int propertyId, int rating, string comment)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Challenge();
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5 stars.";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Please write a comment for your review.";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }

            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null)
            {
                return NotFound();
            }

            var review = new Review
            {
                PropertyId = propertyId,
                ClientId = currentUserId,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);

            // Notify Property Seller
            _context.Notifications.Add(new Notification
            {
                UserId = property.SellerId,
                Title = "New Property Review",
                Message = $"A client left a {rating}-star rating on your listing '{property.Title}'.",
                LinkUrl = $"/Property/Details/{property.Id}#reviews",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Thank you! Your review has been published.";
            return RedirectToAction("Details", "Property", new { id = propertyId });
        }

        // POST: /Review/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && review.ClientId != currentUserId)
            {
                return Forbid();
            }

            var propertyId = review.PropertyId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Review removed.";
            return RedirectToAction("Details", "Property", new { id = propertyId });
        }
    }
}
