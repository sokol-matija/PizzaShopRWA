using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Pages.Destinations
{
    /// <summary>
    /// Page model for displaying all destinations
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IDestinationService _destinationService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IDestinationService destinationService, ILogger<IndexModel> logger)
        {
            _destinationService = destinationService;
            _logger = logger;
        }

        /// <summary>
        /// List of destinations to display
        /// </summary>
        public List<DestinationModel> Destinations { get; set; } = new List<DestinationModel>();
        
        /// <summary>
        /// Error message if API call fails
        /// </summary>
        public string? ErrorMessage { get; set; }

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
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading destinations");
                ErrorMessage = "Failed to load destinations. Please try again later.";
                return Page();
            }
        }

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
    }
} 