using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Admin.Guides
{
    /// <summary>
    /// Page model for editing existing guides
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IGuideService _guideService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IGuideService guideService,
            ILogger<EditModel> logger)
        {
            _guideService = guideService;
            _logger = logger;
        }

        [BindProperty]
        public EditGuideModel Guide { get; set; } = new EditGuideModel();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var guide = await _guideService.GetGuideByIdAsync(id);
            if (guide == null)
            {
                return NotFound();
            }

            // Map GuideModel to EditGuideModel
            Guide = new EditGuideModel
            {
                Id = guide.Id,
                FirstName = guide.FirstName,
                LastName = guide.LastName,
                Bio = guide.Bio,
                Email = guide.Email,
                PhoneNumber = guide.PhoneNumber,
                YearsExperience = guide.YearsExperience
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Map EditGuideModel to GuideModel
                var guide = new GuideModel
                {
                    Id = Guide.Id,
                    FirstName = Guide.FirstName,
                    LastName = Guide.LastName,
                    Bio = Guide.Bio,
                    Email = Guide.Email,
                    PhoneNumber = Guide.PhoneNumber,
                    YearsExperience = Guide.YearsExperience,
                    PhotoUrl = null // No photos for now
                };

                var result = await _guideService.UpdateGuideAsync(Guide.Id, guide);
                if (result != null)
                {
                    _logger.LogInformation("Successfully updated guide: {Name}", guide.FullName);
                    TempData["SuccessMessage"] = "Guide updated successfully!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Failed to update guide: {Name}", guide.FullName);
                    ModelState.AddModelError("", "Failed to update guide. Please try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guide: {Name}", Guide.FullName);
                ModelState.AddModelError("", "An error occurred while updating the guide. Please try again.");
                return Page();
            }
        }

        /// <summary>
        /// AJAX handler for real-time form validation
        /// </summary>
        public async Task<IActionResult> OnPostValidateAsync([FromBody] EditGuideModel guide)
        {
            // Clear model state and validate the incoming model
            ModelState.Clear();
            TryValidateModel(guide);

            var errors = new Dictionary<string, List<string>>();
            
            foreach (var modelError in ModelState)
            {
                if (modelError.Value.Errors.Count > 0)
                {
                    errors[modelError.Key] = modelError.Value.Errors.Select(e => e.ErrorMessage).ToList();
                }
            }

            // Additional business validation - check email uniqueness (excluding current guide)
            if (!string.IsNullOrEmpty(guide.Email))
            {
                try
                {
                    var existingGuides = await _guideService.GetAllGuidesAsync();
                    if (existingGuides.Any(g => g.Id != guide.Id && 
                                               g.Email?.Equals(guide.Email, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        if (!errors.ContainsKey("Guide.Email"))
                            errors["Guide.Email"] = new List<string>();
                        errors["Guide.Email"].Add("Email address is already in use by another guide.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error checking email uniqueness during validation");
                }
            }

            return new JsonResult(new { isValid = errors.Count == 0, errors });
        }

        /// <summary>
        /// AJAX handler for updating guide
        /// </summary>
        public async Task<IActionResult> OnPostUpdateAsync([FromBody] EditGuideModel guide)
        {
            try
            {
                // Debug: Check authentication status
                var isAuth = User.Identity?.IsAuthenticated ?? false;
                var isAdmin = User.IsInRole("Admin");
                var userName = User.Identity?.Name ?? "Unknown";
                _logger.LogInformation("Edit Guide Debug - User: {User}, Authenticated: {Auth}, IsAdmin: {Admin}", 
                    userName, isAuth, isAdmin);
                
                // Validate the model
                ModelState.Clear();
                TryValidateModel(guide);

                if (!ModelState.IsValid)
                {
                    var errors = new Dictionary<string, List<string>>();
                    foreach (var modelError in ModelState)
                    {
                        if (modelError.Value.Errors.Count > 0)
                        {
                            errors[modelError.Key] = modelError.Value.Errors.Select(e => e.ErrorMessage).ToList();
                        }
                    }
                    return new JsonResult(new { success = false, errors });
                }

                // Check email uniqueness (excluding current guide)
                if (!string.IsNullOrEmpty(guide.Email))
                {
                    var existingGuides = await _guideService.GetAllGuidesAsync();
                    if (existingGuides.Any(g => g.Id != guide.Id && 
                                               g.Email?.Equals(guide.Email, StringComparison.OrdinalIgnoreCase) == true))
                    {
                        return new JsonResult(new { 
                            success = false, 
                            errors = new Dictionary<string, List<string>> 
                            { 
                                ["Email"] = new List<string> { "Email address is already in use by another guide." }
                            }
                        });
                    }
                }

                // Update the guide
                var guideModel = new GuideModel
                {
                    Id = guide.Id,
                    FirstName = guide.FirstName,
                    LastName = guide.LastName,
                    Bio = guide.Bio,
                    Email = guide.Email,
                    PhoneNumber = guide.PhoneNumber,
                    YearsExperience = guide.YearsExperience,
                    PhotoUrl = null // No photos for now
                };

                var result = await _guideService.UpdateGuideAsync(guide.Id, guideModel);
                if (result != null)
                {
                    _logger.LogInformation("Successfully updated guide via AJAX: {Name}", guideModel.FullName);
                    return new JsonResult(new { 
                        success = true, 
                        message = "Guide updated successfully!",
                        guide = new {
                            id = result.Id,
                            fullName = result.FullName,
                            email = result.Email
                        }
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to update guide via AJAX: {Name}", guideModel.FullName);
                    return new JsonResult(new { 
                        success = false, 
                        message = "Failed to update guide. Please try again."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating guide via AJAX: {FullName}", guide.FullName);
                return new JsonResult(new { 
                    success = false, 
                    message = "An error occurred while updating the guide. Please try again."
                });
            }
        }
    }

    /// <summary>
    /// View model for editing guide form
    /// </summary>
    public class EditGuideModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Biography cannot be longer than 500 characters")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        public string? PhoneNumber { get; set; }

        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        public int? YearsExperience { get; set; }

        // Computed property for display
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
} 