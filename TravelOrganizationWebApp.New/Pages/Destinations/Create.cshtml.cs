using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Pages.Destinations
{
    /// <summary>
    /// Page model for creating new destinations
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly IDestinationService _destinationService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            IDestinationService destinationService,
            ILogger<CreateModel> logger)
        {
            _destinationService = destinationService;
            _logger = logger;
        }

        [BindProperty]
        public CreateDestinationModel Destination { get; set; } = new CreateDestinationModel();

        public void OnGet()
        {
            // Initialize the form
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var destination = new DestinationModel
                {
                    Name = Destination.Name,
                    Description = Destination.Description ?? string.Empty,
                    Country = Destination.Country,
                    City = Destination.City,
                    ImageUrl = Destination.ImageUrl
                };

                var result = await _destinationService.CreateDestinationAsync(destination);
                if (result != null)
                {
                    _logger.LogInformation("Successfully created destination: {Name}", destination.Name);
                    TempData["SuccessMessage"] = "Destination created successfully!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Failed to create destination: {Name}", destination.Name);
                    ModelState.AddModelError("", "Failed to create destination. Please try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating destination: {Name}", Destination.Name);
                ModelState.AddModelError("", "An error occurred while creating the destination. Please try again.");
                return Page();
            }
        }
    }
} 