using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Pages.Trips
{
    /// <summary>
    /// Page model for booking a trip (requires authentication)
    /// </summary>
    [Authorize]
    public class BookModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;
        private readonly ILogger<BookModel> _logger;

        public BookModel(
            ITripService tripService,
            IDestinationService destinationService,
            ILogger<BookModel> logger)
        {
            _tripService = tripService;
            _destinationService = destinationService;
            _logger = logger;
        }

        public TripModel Trip { get; set; } = default!;
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public int TripId { get; set; }

        [BindProperty]
        [Range(1, 10, ErrorMessage = "Number of participants must be between 1 and 10")]
        public int NumberOfParticipants { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            TripId = id;
            
            try
            {
                _logger.LogInformation("Loading booking page for trip: {TripId}", id);
                
                // Load trip details
                var trip = await _tripService.GetTripByIdAsync(id);
                
                if (trip == null)
                {
                    _logger.LogWarning("Trip not found: {TripId}", id);
                    return NotFound();
                }
                
                // Check if trip is fully booked
                if (trip.CurrentBookings >= trip.Capacity)
                {
                    _logger.LogWarning("Trip is fully booked: {TripId}", id);
                    ErrorMessage = "This trip is fully booked. Please select another trip.";
                    return Page();
                }
                
                Trip = trip;
                
                // If we have a destination ID but no name, get the destination details
                if (trip.DestinationId > 0 && string.IsNullOrEmpty(trip.DestinationName))
                {
                    var destination = await _destinationService.GetDestinationByIdAsync(trip.DestinationId);
                    if (destination != null)
                    {
                        trip.DestinationName = destination.Name;
                    }
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking page for trip: {TripId}", id);
                ErrorMessage = "An error occurred while loading the booking page. Please try again.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync(TripId);
                return Page();
            }

            try
            {
                _logger.LogInformation("Booking trip: {TripId} with {Participants} participants", TripId, NumberOfParticipants);
                
                // Book the trip
                var result = await _tripService.BookTripAsync(TripId, NumberOfParticipants);
                
                if (result)
                {
                    _logger.LogInformation("Trip booked successfully: {TripId}", TripId);
                    TempData["SuccessMessage"] = "Your trip has been booked successfully!";
                    return RedirectToPage("./MyBookings");
                }
                else
                {
                    _logger.LogWarning("Failed to book trip: {TripId}", TripId);
                    ErrorMessage = "Failed to book the trip. Please try again.";
                    await OnGetAsync(TripId);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error booking trip: {TripId}", TripId);
                ErrorMessage = $"An error occurred while booking the trip: {ex.Message}";
                await OnGetAsync(TripId);
                return Page();
            }
        }
    }
} 