using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Trips
{
    /// <summary>
    /// Page model for displaying detailed information about a specific trip
    /// </summary>
    public class DetailsModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;

        public DetailsModel(ITripService tripService, IDestinationService destinationService)
        {
            _tripService = tripService;
            _destinationService = destinationService;
        }

        /// <summary>
        /// The trip to display
        /// </summary>
        public TripModel? Trip { get; set; }
        
        /// <summary>
        /// Destination information for the trip
        /// </summary>
        public DestinationModel? Destination { get; set; }
        
        /// <summary>
        /// Error message if API call fails
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Handle GET request to load trip details
        /// </summary>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Get trip details
                Trip = await _tripService.GetTripByIdAsync(id.Value);
                
                if (Trip == null)
                {
                    return NotFound();
                }
                
                // Get destination details for this trip
                Destination = await _destinationService.GetDestinationByIdAsync(Trip.DestinationId);
                
                // Store destination name in trip object
                if (Destination != null)
                {
                    Trip.DestinationName = Destination.Name;
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading trip details: {ex.Message}";
                return Page();
            }
        }
    }
} 