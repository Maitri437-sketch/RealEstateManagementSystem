using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.ViewModels;
using System.Security.Claims;

namespace RealEstateManagementSystem.Controllers
{
    [Authorize]
    public class ComparisonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ComparisonController> _logger;

        public ComparisonController(
            ApplicationDbContext context,
            ILogger<ComparisonController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Comparison
        public async Task<IActionResult> Index()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                return Challenge();
            }

            var comparisons = await _context.PropertyComparisons
                .Include(c => c.Property)
                    .ThenInclude(p => p!.Images)
                .Include(c => c.Property)
                    .ThenInclude(p => p!.Seller)
                .Include(c => c.Property)
                    .ThenInclude(p => p!.Agent)
                .Where(c => c.ClientId == clientId)
                .OrderBy(c => c.AddedAt)
                .ToListAsync();

            var model = new ComparisonViewModel
            {
                Properties = comparisons.Select(c => c.Property!).Where(p => p != null).ToList()
            };

            return View(model);
        }

        // GET or POST: /Comparison/Add?propertyId=5
        public async Task<IActionResult> Add(int propertyId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                return Challenge();
            }

            // Check if already added
            var exists = await _context.PropertyComparisons
                .AnyAsync(c => c.ClientId == clientId && c.PropertyId == propertyId);

            if (exists)
            {
                TempData["InfoMessage"] = "This property is already in your comparison table.";
                return RedirectToAction(nameof(Index));
            }

            // Check max 4 count limit
            var count = await _context.PropertyComparisons
                .CountAsync(c => c.ClientId == clientId);

            if (count >= ComparisonViewModel.MaxComparisonCount)
            {
                TempData["ErrorMessage"] = "You can compare up to 4 properties simultaneously. Please remove a property from your list before adding another.";
                return RedirectToAction(nameof(Index));
            }

            var prop = await _context.Properties.FindAsync(propertyId);
            if (prop != null)
            {
                _context.PropertyComparisons.Add(new PropertyComparison
                {
                    ClientId = clientId,
                    PropertyId = propertyId,
                    AddedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{prop.Title}' added to comparison list.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Comparison/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int propertyId)
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                return Challenge();
            }

            var item = await _context.PropertyComparisons
                .FirstOrDefaultAsync(c => c.ClientId == clientId && c.PropertyId == propertyId);

            if (item != null)
            {
                _context.PropertyComparisons.Remove(item);
                await _context.SaveChangesAsync();
                TempData["InfoMessage"] = "Property removed from comparison.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Comparison/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var clientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(clientId))
            {
                return Challenge();
            }

            var items = await _context.PropertyComparisons
                .Where(c => c.ClientId == clientId)
                .ToListAsync();

            if (items.Any())
            {
                _context.PropertyComparisons.RemoveRange(items);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Comparison list cleared.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
