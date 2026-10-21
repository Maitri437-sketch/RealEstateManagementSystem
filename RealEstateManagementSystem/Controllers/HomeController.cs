using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateManagementSystem.Data;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.ViewModels;

namespace RealEstateManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProperties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.Status == "Approved" && p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();

            var latestProperties = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();

            var categoryCounts = await _context.Properties
                .Where(p => p.Status == "Approved")
                .GroupBy(p => p.PropertyType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Type, g => g.Count);

            var userWishlistIds = new HashSet<int>();
            var userComparisonIds = new HashSet<int>();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    userWishlistIds = (await _context.Wishlists
                        .Where(w => w.ClientId == userId)
                        .Select(w => w.PropertyId)
                        .ToListAsync()).ToHashSet();

                    userComparisonIds = (await _context.PropertyComparisons
                        .Where(c => c.ClientId == userId)
                        .Select(c => c.PropertyId)
                        .ToListAsync()).ToHashSet();
                }
            }

            var viewModel = new HomeIndexViewModel
            {
                FeaturedProperties = featuredProperties,
                LatestProperties = latestProperties,
                CategoryCounts = categoryCounts,
                UserWishlistIds = userWishlistIds,
                UserComparisonIds = userComparisonIds
            };

            return View(viewModel);
        }

        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                ModelState.AddModelError(string.Empty, "Please fill in all required fields.");
                return View();
            }

            TempData["SuccessMessage"] = "Thank you for contacting us! Our team will respond to your inquiry shortly.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
