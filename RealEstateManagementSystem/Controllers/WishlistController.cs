using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using System.Security.Claims;

namespace RealEstateManagementSystem.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WishlistController> _logger;

        public WishlistController(
            ApplicationDbContext context,
            ILogger<WishlistController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Wishlist
        public async Task<IActionResult> Index()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                return Challenge();
            }

            var wishlists = await _context.Wishlists
                .Include(w => w.Property)
                    .ThenInclude(p => p!.Images)
                .Include(w => w.Property)
                    .ThenInclude(p => p!.Reviews)
                .Where(w => w.ClientId == clientId)
                .OrderByDescending(w => w.AddedAt)
                .ToListAsync();

            return View(wishlists);
        }

        // GET or POST: /Wishlist/Toggle?propertyId=5
        public async Task<IActionResult> Toggle(int propertyId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                return Challenge();
            }

            var existing = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.ClientId == clientId && w.PropertyId == propertyId);

            if (existing != null)
            {
                // Remove from wishlist
                _context.Wishlists.Remove(existing);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = "Property removed from your saved wishlist.";
            }
            else
            {
                // Check if property exists
                var prop = await _context.Properties.FindAsync(propertyId);
                if (prop != null)
                {
                    _context.Wishlists.Add(new Wishlist
                    {
                        ClientId = clientId,
                        PropertyId = propertyId,
                        AddedAt = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"'{prop.Title}' saved to your wishlist!";
                }
            }

            // Return to referring URL if available
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int propertyId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                return Challenge();
            }

            var item = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.ClientId == clientId && w.PropertyId == propertyId);

            if (item != null)
            {
                _context.Wishlists.Remove(item);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = "Property removed from wishlist.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
