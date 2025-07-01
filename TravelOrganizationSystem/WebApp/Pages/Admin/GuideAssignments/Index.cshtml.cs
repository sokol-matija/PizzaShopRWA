using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Admin.GuideAssignments
{
    /// <summary>
    /// Admin page for managing guide assignments to trips
    /// Simple interface to assign and remove guides from trips
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly IGuideService _guideService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ITripService tripService, IGuideService guideService, ILogger<IndexModel> logger)
        {
            _tripService = tripService;
            _guideService = guideService;
            _logger = logger;
        }

        /// <summary>
        /// List of all trips with their assigned guides
        /// </summary>
        public List<TripModel> Trips { get; set; } = new List<TripModel>();

        /// <summary>
        /// List of all available guides
        /// </summary>
        public List<GuideModel> Guides { get; set; } = new List<GuideModel>();

        /// <summary>
        /// Select list for trips dropdown
        /// </summary>
        public SelectList TripSelectList { get; set; } = default!;

        /// <summary>
        /// Select list for guides dropdown  
        /// </summary>
        public SelectList GuideSelectList { get; set; } = default!;

        /// <summary>
        /// Error message to display if something goes wrong
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Success message to display when operations complete
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Load the guide assignment page
        /// </summary>
        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading guide assignments page");
                
                // Load trips and guides
                await LoadDataAsync();
                
                _logger.LogInformation("Loaded {TripCount} trips and {GuideCount} guides", 
                    Trips.Count, Guides.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading guide assignments page");
                ErrorMessage = "Unable to load guide assignments. Please try again later.";
            }
        }

        /// <summary>
        /// Handle guide assignment via AJAX POST
        /// </summary>
        public async Task<IActionResult> OnPostAssignAsync(int tripId, int guideId)
        {
            try
            {
                _logger.LogInformation("Admin attempting to assign guide {GuideId} to trip {TripId}", guideId, tripId);
                
                // Validate inputs
                if (tripId <= 0 || guideId <= 0)
                {
                    return new JsonResult(new { success = false, message = "Invalid trip or guide ID." });
                }
                
                // Attempt to assign the guide
                var success = await _tripService.AssignGuideToTripAsync(tripId, guideId);
                
                if (success)
                {
                    _logger.LogInformation("Successfully assigned guide {GuideId} to trip {TripId}", guideId, tripId);
                    
                    // Get guide and trip names for success message
                    var guide = await _guideService.GetGuideByIdAsync(guideId);
                    var trip = await _tripService.GetTripByIdAsync(tripId);
                    
                    var message = $"Successfully assigned {guide?.FullName ?? "guide"} to {trip?.Title ?? "trip"}.";
                    
                    return new JsonResult(new { success = true, message });
                }
                else
                {
                    _logger.LogWarning("Failed to assign guide {GuideId} to trip {TripId}", guideId, tripId);
                    return new JsonResult(new { success = false, message = "Failed to assign guide. The guide may already be assigned to this trip." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while assigning guide {GuideId} to trip {TripId}", guideId, tripId);
                return new JsonResult(new { success = false, message = "An error occurred while assigning the guide." });
            }
        }

        /// <summary>
        /// Handle guide removal via AJAX POST
        /// </summary>
        public async Task<IActionResult> OnPostRemoveAsync(int tripId, int guideId)
        {
            try
            {
                _logger.LogInformation("Admin attempting to remove guide {GuideId} from trip {TripId}", guideId, tripId);
                
                // Validate inputs
                if (tripId <= 0 || guideId <= 0)
                {
                    return new JsonResult(new { success = false, message = "Invalid trip or guide ID." });
                }
                
                // Attempt to remove the guide
                var success = await _tripService.RemoveGuideFromTripAsync(tripId, guideId);
                
                if (success)
                {
                    _logger.LogInformation("Successfully removed guide {GuideId} from trip {TripId}", guideId, tripId);
                    
                    // Get guide and trip names for success message
                    var guide = await _guideService.GetGuideByIdAsync(guideId);
                    var trip = await _tripService.GetTripByIdAsync(tripId);
                    
                    var message = $"Successfully removed {guide?.FullName ?? "guide"} from {trip?.Title ?? "trip"}.";
                    
                    return new JsonResult(new { success = true, message });
                }
                else
                {
                    _logger.LogWarning("Failed to remove guide {GuideId} from trip {TripId}", guideId, tripId);
                    return new JsonResult(new { success = false, message = "Failed to remove guide. The guide may not be assigned to this trip." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while removing guide {GuideId} from trip {TripId}", guideId, tripId);
                return new JsonResult(new { success = false, message = "An error occurred while removing the guide." });
            }
        }

        /// <summary>
        /// Load trips and guides data for the page
        /// </summary>
        private async Task LoadDataAsync()
        {
            // Load all trips with their guides
            Trips = await _tripService.GetAllTripsAsync();
            
            // Load all guides and convert to List
            Guides = (await _guideService.GetAllGuidesAsync()).ToList();
            
            // Create select lists for dropdowns
            TripSelectList = new SelectList(Trips, nameof(TripModel.Id), nameof(TripModel.Title));
            GuideSelectList = new SelectList(Guides, nameof(GuideModel.Id), nameof(GuideModel.FullName));
        }
    }
} 