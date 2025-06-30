using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Admin.Guides
{
    /// <summary>
    /// Page model for creating new guides
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IGuideService _guideService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IGuideService guideService,
            ILogger<CreateModel> logger)
        {
            _guideService = guideService;
            _logger = logger;
        }

        [BindProperty]
        public CreateGuideModel Guide { get; set; } = new CreateGuideModel();

        public void OnGet()
        {
            // Initialize form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Map CreateGuideModel to GuideModel
                var guide = new GuideModel
                {
                    FirstName = Guide.FirstName,
                    LastName = Guide.LastName,
                    Bio = Guide.Bio,
                    Email = Guide.Email,
                    PhoneNumber = Guide.PhoneNumber,
                    YearsExperience = Guide.YearsExperience,
                    PhotoUrl = null // No photos for now
                };

                var result = await _guideService.CreateGuideAsync(guide);
                if (result != null)
                {
                    _logger.LogInformation("Successfully created guide: {Name}", guide.FullName);
                    TempData["SuccessMessage"] = "Guide created successfully!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Failed to create guide: {Name}", guide.FullName);
                    ModelState.AddModelError("", "Failed to create guide. Please try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating guide: {Name}", Guide.FullName);
                ModelState.AddModelError("", "An error occurred while creating the guide. Please try again.");
                return Page();
            }
        }

        /// <summary>
        /// AJAX handler for real-time form validation
        /// </summary>
        public async Task<IActionResult> OnPostValidateAsync([FromBody] CreateGuideModel guide)
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

            // Additional business validation
            if (!string.IsNullOrEmpty(guide.Email))
            {
                try
                {
                    // Check if email already exists
                    var existingGuides = await _guideService.GetAllGuidesAsync();
                    if (existingGuides.Any(g => g.Email?.Equals(guide.Email, StringComparison.OrdinalIgnoreCase) == true))
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
        /// AJAX handler for creating guide
        /// </summary>
        public async Task<IActionResult> OnPostCreateAsync([FromBody] CreateGuideModel guide)
        {
            try
            {
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

                // Check email uniqueness
                if (!string.IsNullOrEmpty(guide.Email))
                {
                    var existingGuides = await _guideService.GetAllGuidesAsync();
                    if (existingGuides.Any(g => g.Email?.Equals(guide.Email, StringComparison.OrdinalIgnoreCase) == true))
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

                // Create the guide
                var guideModel = new GuideModel
                {
                    FirstName = guide.FirstName,
                    LastName = guide.LastName,
                    Bio = guide.Bio,
                    Email = guide.Email,
                    PhoneNumber = guide.PhoneNumber,
                    YearsExperience = guide.YearsExperience,
                    PhotoUrl = null // No photos for now
                };

                var result = await _guideService.CreateGuideAsync(guideModel);
                if (result != null)
                {
                    _logger.LogInformation("Successfully created guide via AJAX: {Name}", guideModel.FullName);
                    return new JsonResult(new { 
                        success = true, 
                        message = "Guide created successfully!",
                        guide = new {
                            id = result.Id,
                            fullName = result.FullName,
                            email = result.Email
                        }
                    });
                }
                else
                {
                    _logger.LogWarning("Failed to create guide via AJAX: {Name}", guideModel.FullName);
                    return new JsonResult(new { 
                        success = false, 
                        message = "Failed to create guide. Please try again."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating guide via AJAX: {Name}", guide.FullName);
                return new JsonResult(new { 
                    success = false, 
                    message = "An error occurred while creating the guide. Please try again."
                });
            }
        }
    }

    /// <summary>
    /// Model for creating a new guide
    /// </summary>
    public class CreateGuideModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Biography cannot be longer than 500 characters")]
        [Display(Name = "Biography")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        [Display(Name = "Years of Experience")]
        public int? YearsExperience { get; set; }

        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
} 