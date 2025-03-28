using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;
using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Pages.Trips
{
    /// <summary>
    /// Page model for displaying all trips with optional destination filtering
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;

        public IndexModel(ITripService tripService, IDestinationService destinationService)
        {
            _tripService = tripService;
            _destinationService = destinationService;
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
        /// Error message if API call fails
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Handle GET request to load trips
        /// </summary>
        public async Task OnGetAsync()
        {
            try
            {
                // Get all destinations for the filter dropdown
                var destinations = await _destinationService.GetAllDestinationsAsync();
                Destinations = new SelectList(destinations, nameof(DestinationModel.Id), nameof(DestinationModel.Name));
                
                // Get trips (filtered by destination if selected)
                if (DestinationId.HasValue && DestinationId.Value > 0)
                {
                    Trips = await _tripService.GetTripsByDestinationAsync(DestinationId.Value);
                }
                else
                {
                    Trips = await _tripService.GetAllTripsAsync();
                }
                
                // Fetch destination names for each trip 
                foreach (var trip in Trips)
                {
                    var destination = destinations.FirstOrDefault(d => d.Id == trip.DestinationId);
                    if (destination != null)
                    {
                        trip.DestinationName = destination.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading trips: {ex.Message}";
            }
        }
    }
} 