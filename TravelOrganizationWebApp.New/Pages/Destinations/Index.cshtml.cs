using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Pages.Destinations
{
    /// <summary>
    /// Page model for displaying all destinations
    /// </summary>
    public class DestinationsIndexModel : PageModel
    {
        private readonly IDestinationService _destinationService;
        private readonly IUnsplashService _unsplashService;
        private readonly ILogger<DestinationsIndexModel> _logger;

        public DestinationsIndexModel(
            IDestinationService destinationService,
            IUnsplashService unsplashService,
            ILogger<DestinationsIndexModel> logger)
        {
            _destinationService = destinationService;
            _unsplashService = unsplashService;
            _logger = logger;
        }

        /// <summary>
        /// List of destinations to display
        /// </summary>
        public List<DestinationModel> Destinations { get; set; } = new List<DestinationModel>();
        
        /// <summary>
        /// Error message if API call fails
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        [BindProperty]
        public CreateDestinationModel NewDestination { get; set; } = new CreateDestinationModel();

        /// <summary>
        /// Handle GET request to load destinations
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Destinations = await _destinationService.GetAllDestinationsAsync();
                
                // Get images for destinations that don't have one
                foreach (var destination in Destinations.Where(d => string.IsNullOrEmpty(d.ImageUrl)))
                {
                    var imageUrl = await _unsplashService.GetRandomImageUrlAsync($"{destination.City} {destination.Country}");
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        destination.ImageUrl = imageUrl;
                    }
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading destinations");
                ErrorMessage = "Failed to load destinations. Please try again later.";
                return Page();
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid)
            {
                Destinations = await _destinationService.GetAllDestinationsAsync();
                return Page();
            }

            try
            {
                var destination = new DestinationModel
                {
                    Name = NewDestination.Name,
                    Description = NewDestination.Description,
                    Country = NewDestination.Country,
                    City = NewDestination.City,
                    Climate = NewDestination.Climate,
                    BestTimeToVisit = NewDestination.BestTimeToVisit
                };

                var result = await _destinationService.CreateDestinationAsync(destination);
                if (result != null)
                {
                    _logger.LogInformation("Successfully created destination: {Name}", destination.Name);
                    TempData["SuccessMessage"] = "Destination created successfully!";
                    return RedirectToPage();
                }
                else
                {
                    _logger.LogWarning("Failed to create destination: {Name}", destination.Name);
                    TempData["ErrorMessage"] = "Failed to create destination. Please try again.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating destination: {Name}", NewDestination.Name);
                TempData["ErrorMessage"] = "An error occurred while creating the destination. Please try again.";
                return Page();
            }
        }

        /// <summary>
        /// Handle POST request to delete a destination (for admin users)
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                await _destinationService.DeleteDestinationAsync(id);
                _logger.LogInformation("Destination deleted successfully: {Id}", id);
                TempData["SuccessMessage"] = "Destination deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting destination: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the destination.";
            }

            return RedirectToPage();
        }
    }
} 