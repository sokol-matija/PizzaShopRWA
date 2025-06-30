using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Admin.Guides
{
    /// <summary>
    /// Page model for viewing guide details
    /// Provides comprehensive information about a specific guide
    /// </summary>
    public class DetailsModel : PageModel
    {
        private readonly IGuideService _guideService;
        private readonly ITripService _tripService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IGuideService guideService,
            ITripService tripService,
            ILogger<DetailsModel> logger)
        {
            _guideService = guideService;
            _tripService = tripService;
            _logger = logger;
        }

        /// <summary>
        /// The guide to display details for
        /// </summary>
        public GuideModel Guide { get; set; } = default!;

        /// <summary>
        /// Number of trips this guide is assigned to
        /// </summary>
        public int TripsCount { get; set; }

        /// <summary>
        /// Error message to display if something goes wrong
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Load guide details page
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                _logger.LogInformation("Loading guide details for ID: {GuideId}", id);
                
                // Get guide details
                var guide = await _guideService.GetGuideByIdAsync(id);
                
                if (guide == null)
                {
                    _logger.LogWarning("Guide not found: {GuideId}", id);
                    return NotFound();
                }

                Guide = guide;

                // Set trips count to 0 for now - this functionality can be implemented later
                // when the GetTripsByGuideAsync method is available in the trip service
                TripsCount = 0;

                _logger.LogInformation("Successfully loaded guide details: {GuideName}", guide.FullName);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving guide details: {GuideId}", id);
                ErrorMessage = "An error occurred while retrieving the guide details.";
                return RedirectToPage("./Index");
            }
        }
    }
} 