using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Trips
{
    /// <summary>
    /// Page model for displaying all trips with optional destination filtering and pagination
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            ITripService tripService, 
            IDestinationService destinationService,
            ILogger<IndexModel> logger)
        {
            _tripService = tripService;
            _destinationService = destinationService;
            _logger = logger;
        }

        /// <summary>
        /// List of trips to display
        /// </summary>
        public List<TripModel> Trips { get; set; } = new List<TripModel>();
        
        /// <summary>
        /// Available destinations for filtering
        /// </summary>
        public SelectList Destinations { get; set; } = default!;
        
        /// <summary>
        /// Selected destination ID for filtering
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public int? DestinationId { get; set; }
        
        /// <summary>
        /// Current page number
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public new int Page { get; set; } = 1;
        
        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; } = 10;
        
        /// <summary>
        /// Total number of trips
        /// </summary>
        public int TotalTrips { get; set; }
        
        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }
        
        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage => Page > 1;
        
        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage => Page < TotalPages;
        
        /// <summary>
        /// Error message if API call fails
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Handle GET request to load trips with pagination
        /// </summary>
        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading trips page with destination filter: {DestinationId}, Page: {Page}", DestinationId, Page);
                
                // Ensure page is valid
                if (Page < 1) Page = 1;
                
                // Get all destinations for the filter dropdown
                var destinations = await _destinationService.GetAllDestinationsAsync();
                Destinations = new SelectList(destinations, nameof(DestinationModel.Id), nameof(DestinationModel.Name));
                
                _logger.LogInformation("Loaded {Count} destinations for filter", destinations.Count);
                
                // Get trips with pagination
                var (trips, totalCount) = await _tripService.GetTripsAsync(Page, PageSize, DestinationId);
                
                Trips = trips;
                TotalTrips = totalCount;
                TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);
                
                _logger.LogInformation("Loaded {Count} trips out of {Total} for page {Page}/{TotalPages}", 
                    Trips.Count, TotalTrips, Page, TotalPages);
                
                // Fetch destination names for each trip (should already be included from API)
                foreach (var trip in Trips)
                {
                    var destination = destinations.FirstOrDefault(d => d.Id == trip.DestinationId);
                    if (destination != null && string.IsNullOrEmpty(trip.DestinationName))
                    {
                        trip.DestinationName = destination.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trips");
                ErrorMessage = $"Error loading trips: {ex.Message}";
                
                // Set default values to prevent errors in the view
                Trips = new List<TripModel>();
                TotalTrips = 0;
                TotalPages = 0;
            }
        }

        /// <summary>
        /// Handle POST request to delete a trip (for admin users)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting trip: {Id}", id);
                var result = await _tripService.DeleteTripAsync(id);
                
                if (result)
                {
                    _logger.LogInformation("Trip deleted successfully: {Id}", id);
                    TempData["SuccessMessage"] = "Trip deleted successfully.";
                }
                else
                {
                    _logger.LogWarning("Failed to delete trip: {Id}", id);
                    TempData["ErrorMessage"] = "Failed to delete the trip. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting trip: {Id}", id);
                TempData["ErrorMessage"] = $"An error occurred while deleting the trip: {ex.Message}";
            }

            // Redirect to current page with current filters
            return RedirectToPage(new { DestinationId = DestinationId, Page = Page });
        }
    }
} 