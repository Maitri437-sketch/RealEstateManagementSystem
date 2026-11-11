using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealEstateManagementSystem.Models;
using RealEstateManagementSystem.ViewModels;
using System.Security.Claims;

namespace RealEstateManagementSystem.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProfileController> _logger;

        private readonly string[] _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        public ProfileController(
            UserManager<Users> userManager,
            SignInManager<Users> signInManager,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        // GET: /Profile
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Member";

            var model = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                CurrentProfilePictureUrl = user.ProfilePictureUrl,
                Role = primaryRole,
                MemberSince = user.CreatedAt
            };

            return View(model);
        }

        // POST: /Profile/Update
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Full name is required.");
            }

            // Optional password change validation
            bool changingPassword = !string.IsNullOrEmpty(model.NewPassword);
            if (changingPassword)
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is required to set a new password.");
                }
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    ModelState.AddModelError("ConfirmNewPassword", "New password and confirmation do not match.");
                }
            }

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName.Trim();
                user.PhoneNumber = model.PhoneNumber?.Trim();
                user.Address = model.Address?.Trim();

                // Profile Image Upload
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    if (model.ProfilePicture.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ProfilePicture", "Profile picture file size cannot exceed 5MB.");
                        return View("Index", model);
                    }

                    var ext = Path.GetExtension(model.ProfilePicture.FileName).ToLowerInvariant();
                    if (!_allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("ProfilePicture", "Invalid image format. Allowed formats: JPG, JPEG, PNG, WEBP.");
                        return View("Index", model);
                    }

                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                    Directory.CreateDirectory(uploadsFolder);

                    // Delete old profile picture from disk if it was a custom upload
                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && user.ProfilePictureUrl.StartsWith("/uploads/profiles/"))
                    {
                        var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            try { System.IO.File.Delete(oldFilePath); } catch { /* ignore */ }
                        }
                    }

                    string uniqueFileName = $"{Guid.NewGuid()}{ext}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    user.ProfilePictureUrl = $"/uploads/profiles/{uniqueFileName}";
                }

                // Update User Details
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("Index", model);
                }

                // Handle Password Change if requested
                if (changingPassword)
                {
                    var passwordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword!);
                    if (!passwordResult.Succeeded)
                    {
                        foreach (var error in passwordResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("Index", model);
                    }

                    await _signInManager.RefreshSignInAsync(user);
                }

                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            model.Role = roles.FirstOrDefault() ?? "Member";
            model.Email = user.Email ?? string.Empty;
            model.MemberSince = user.CreatedAt;
            model.CurrentProfilePictureUrl = user.ProfilePictureUrl;

            return View("Index", model);
        }
    }
}
