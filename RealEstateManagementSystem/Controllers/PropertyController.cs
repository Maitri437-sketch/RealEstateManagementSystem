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
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<PropertyController> _logger;

        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        public PropertyController(
            ApplicationDbContext context,
            UserManager<Users> userManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<PropertyController> logger)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: /Property
        [HttpGet]
        public async Task<IActionResult> Index(PropertySearchViewModel filter)
        {
            var query = _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .AsQueryable();

            // By default, public search only shows Approved properties
            if (User.IsInRole("Admin") && !string.IsNullOrEmpty(filter.SearchTerm))
            {
                // Admin can view all statuses if explicitly requested
            }
            else
            {
                query = query.Where(p => p.Status == "Approved");
            }

            // Keyword filter
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(term) ||
                                         p.Description.ToLower().Contains(term) ||
                                         p.Address.ToLower().Contains(term) ||
                                         p.City.ToLower().Contains(term) ||
                                         p.State.ToLower().Contains(term));
            }

            // City filter
            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                query = query.Where(p => p.City.ToLower().Contains(filter.City.Trim().ToLower()));
            }

            // Property type filter
            if (!string.IsNullOrWhiteSpace(filter.PropertyType))
            {
                query = query.Where(p => p.PropertyType == filter.PropertyType);
            }

            // Listing type filter (Sale / Rent)
            if (!string.IsNullOrWhiteSpace(filter.ListingType) && filter.ListingType != "All")
            {
                query = query.Where(p => p.ListingType == filter.ListingType);
            }

            // Price range filter
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filter.MinPrice.Value);
            }
            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);
            }

            // Bedrooms & Bathrooms
            if (filter.MinBedrooms.HasValue && filter.MinBedrooms.Value > 0)
            {
                query = query.Where(p => p.Bedrooms >= filter.MinBedrooms.Value);
            }
            if (filter.MinBathrooms.HasValue && filter.MinBathrooms.Value > 0)
            {
                query = query.Where(p => p.Bathrooms >= filter.MinBathrooms.Value);
            }

            // Sorting
            query = filter.SortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "area_desc" => query.OrderByDescending(p => p.AreaSqFt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Pagination
            filter.TotalItems = await query.CountAsync();
            if (filter.CurrentPage < 1) filter.CurrentPage = 1;
            filter.PageSize = 9;

            filter.Properties = await query
                .Skip((filter.CurrentPage - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Load wishlist and comparison states for logged in user
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    filter.UserWishlistPropertyIds = (await _context.Wishlists
                        .Where(w => w.ClientId == userId)
                        .Select(w => w.PropertyId)
                        .ToListAsync()).ToHashSet();

                    filter.UserComparisonPropertyIds = (await _context.PropertyComparisons
                        .Where(c => c.ClientId == userId)
                        .Select(c => c.PropertyId)
                        .ToListAsync()).ToHashSet();
                }
            }

            return View(filter);
        }

        // GET: /Property/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .Include(p => p.Agent)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.Client)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            // If not approved, verify user is owner or admin or assigned agent
            if (property.Status != "Approved")
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = User.IsInRole("Admin");
                var isOwner = property.SellerId == currentUserId;
                var isAssignedAgent = property.AgentId == currentUserId;

                if (!isAdmin && !isOwner && !isAssignedAgent)
                {
                    TempData["ErrorMessage"] = "This property is currently pending approval and is not publicly listed yet.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Check if user has wishlisted or added to comparison
            ViewBag.IsWishlisted = false;
            ViewBag.IsCompared = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    ViewBag.IsWishlisted = await _context.Wishlists.AnyAsync(w => w.PropertyId == id && w.ClientId == userId);
                    ViewBag.IsCompared = await _context.PropertyComparisons.AnyAsync(c => c.PropertyId == id && c.ClientId == userId);
                }
            }

            // Related properties
            ViewBag.RelatedProperties = await _context.Properties
                .Include(p => p.Images)
                .Where(p => p.Id != id && p.Status == "Approved" && (p.PropertyType == property.PropertyType || p.City == property.City))
                .Take(3)
                .ToListAsync();

            return View(property);
        }

        // GET: /Property/Create
        [HttpGet]
        [Authorize(Roles = "Admin,Agent,Seller")]
        public async Task<IActionResult> Create()
        {
            var model = new PropertyViewModel();
            await PopulateAgentsDropdownAsync(model);
            return View(model);
        }

        // POST: /Property/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Agent,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PropertyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Challenge();
                }

                // If user is Admin, they can approve directly. Sellers/Agents default to Pending
                string initialStatus = User.IsInRole("Admin") ? "Approved" : "Pending";

                var property = new Property
                {
                    Title = model.Title.Trim(),
                    Description = model.Description,
                    PropertyType = model.PropertyType,
                    ListingType = model.ListingType,
                    Price = model.Price,
                    Address = model.Address.Trim(),
                    City = model.City.Trim(),
                    State = model.State.Trim(),
                    ZipCode = model.ZipCode.Trim(),
                    Bedrooms = model.Bedrooms,
                    Bathrooms = model.Bathrooms,
                    AreaSqFt = model.AreaSqFt,
                    YearBuilt = model.YearBuilt,
                    Status = initialStatus,
                    IsFeatured = User.IsInRole("Admin") && model.IsFeatured,
                    SellerId = currentUserId,
                    AgentId = model.AgentId,
                    CreatedAt = DateTime.UtcNow
                };

                // Handle Image Uploads securely
                if (model.Images != null && model.Images.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "properties");
                    Directory.CreateDirectory(uploadsFolder);

                    bool isFirst = true;
                    foreach (var file in model.Images)
                    {
                        if (file.Length > 0 && file.Length < 10 * 1024 * 1024) // 10MB limit
                        {
                            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                            if (_allowedExtensions.Contains(ext))
                            {
                                string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }

                                property.Images.Add(new PropertyImage
                                {
                                    ImageUrl = $"/uploads/properties/{uniqueFileName}",
                                    IsMain = isFirst,
                                    UploadedAt = DateTime.UtcNow
                                });

                                isFirst = false;
                            }
                        }
                    }
                }

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                if (initialStatus == "Pending")
                {
                    TempData["SuccessMessage"] = "Your property listing has been submitted successfully and is awaiting Admin review before being published.";
                    return RedirectToAction("Dashboard", User.IsInRole("Seller") ? "Seller" : "Agent");
                }
                else
                {
                    TempData["SuccessMessage"] = "Property has been created and published successfully!";
                    return RedirectToAction(nameof(Details), new { id = property.Id });
                }
            }

            await PopulateAgentsDropdownAsync(model);
            return View(model);
        }

        // GET: /Property/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin,Agent,Seller")]
        public async Task<IActionResult> Edit(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            // Check permissions
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isOwner = property.SellerId == currentUserId;
            var isAgent = property.AgentId == currentUserId;

            if (!isAdmin && !isOwner && !isAgent)
            {
                return Forbid();
            }

            var model = new PropertyViewModel
            {
                Id = property.Id,
                Title = property.Title,
                Description = property.Description,
                PropertyType = property.PropertyType,
                ListingType = property.ListingType,
                Price = property.Price,
                Address = property.Address,
                City = property.City,
                State = property.State,
                ZipCode = property.ZipCode,
                Bedrooms = property.Bedrooms,
                Bathrooms = property.Bathrooms,
                AreaSqFt = property.AreaSqFt,
                YearBuilt = property.YearBuilt,
                IsFeatured = property.IsFeatured,
                Status = property.Status,
                AgentId = property.AgentId,
                SellerId = property.SellerId,
                ExistingImages = property.Images.ToList()
            };

            await PopulateAgentsDropdownAsync(model);
            return View(model);
        }

        // POST: /Property/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Agent,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PropertyViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            var isOwner = property.SellerId == currentUserId;
            var isAgent = property.AgentId == currentUserId;

            if (!isAdmin && !isOwner && !isAgent)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                property.Title = model.Title.Trim();
                property.Description = model.Description;
                property.PropertyType = model.PropertyType;
                property.ListingType = model.ListingType;
                property.Price = model.Price;
                property.Address = model.Address.Trim();
                property.City = model.City.Trim();
                property.State = model.State.Trim();
                property.ZipCode = model.ZipCode.Trim();
                property.Bedrooms = model.Bedrooms;
                property.Bathrooms = model.Bathrooms;
                property.AreaSqFt = model.AreaSqFt;
                property.YearBuilt = model.YearBuilt;
                property.UpdatedAt = DateTime.UtcNow;

                if (isAdmin)
                {
                    property.Status = model.Status;
                    property.IsFeatured = model.IsFeatured;
                    property.AgentId = model.AgentId;
                }
                else if (isOwner && property.Status == "Rejected")
                {
                    // Re-submit for approval if it was rejected previously
                    property.Status = "Pending";
                }

                // Handle New Image Uploads
                if (model.Images != null && model.Images.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "properties");
                    Directory.CreateDirectory(uploadsFolder);

                    bool hasMain = property.Images.Any(i => i.IsMain);

                    foreach (var file in model.Images)
                    {
                        if (file.Length > 0 && file.Length < 10 * 1024 * 1024)
                        {
                            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                            if (_allowedExtensions.Contains(ext))
                            {
                                string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }

                                property.Images.Add(new PropertyImage
                                {
                                    ImageUrl = $"/uploads/properties/{uniqueFileName}",
                                    IsMain = !hasMain,
                                    UploadedAt = DateTime.UtcNow
                                });

                                hasMain = true;
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Property listing updated successfully!";
                return RedirectToAction(nameof(Details), new { id = property.Id });
            }

            model.ExistingImages = property.Images.ToList();
            await PopulateAgentsDropdownAsync(model);
            return View(model);
        }

        // GET: /Property/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && property.SellerId != currentUserId)
            {
                return Forbid();
            }

            return View(property);
        }

        // POST: /Property/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var property = await _context.Properties
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (property == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Admin") && property.SellerId != currentUserId)
            {
                return Forbid();
            }

            // Remove files from disk
            foreach (var img in property.Images)
            {
                if (!string.IsNullOrEmpty(img.ImageUrl) && img.ImageUrl.StartsWith("/uploads/"))
                {
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        try { System.IO.File.Delete(fullPath); } catch { /* ignore */ }
                    }
                }
            }

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Property has been removed successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Property/DeleteImage
        [HttpPost]
        [Authorize(Roles = "Admin,Agent,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int propertyId)
        {
            var image = await _context.PropertyImages.FindAsync(imageId);
            if (image != null && image.PropertyId == propertyId)
            {
                if (!string.IsNullOrEmpty(image.ImageUrl) && image.ImageUrl.StartsWith("/uploads/"))
                {
                    var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(fullPath))
                    {
                        try { System.IO.File.Delete(fullPath); } catch { /* ignore */ }
                    }
                }

                _context.PropertyImages.Remove(image);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Image removed.";
            }

            return RedirectToAction(nameof(Edit), new { id = propertyId });
        }

        private async Task PopulateAgentsDropdownAsync(PropertyViewModel model)
        {
            var agents = await _userManager.GetUsersInRoleAsync("Agent");
            model.Agents = agents.Select(a => new SelectListItem
            {
                Value = a.Id,
                Text = $"{a.FullName} ({a.Email})"
            }).ToList();
        }
    }
}
