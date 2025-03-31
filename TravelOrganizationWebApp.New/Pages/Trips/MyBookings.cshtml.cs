using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Pages.Trips
{
    /// <summary>
    /// Page model for displaying user's trip bookings
    /// </summary>
    [Authorize]
    public class MyBookingsModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly ILogger<MyBookingsModel> _logger;

        public MyBookingsModel(
            ITripService tripService,
            ILogger<MyBookingsModel> logger)
        {
            _tripService = tripService;
            _logger = logger;
        }

        public List<TripRegistrationModel> Bookings { get; set; } = new List<TripRegistrationModel>();

        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading user's bookings");
                
                // Get all bookings for the current user
                Bookings = await _tripService.GetUserTripsAsync();
                
                _logger.LogInformation("Loaded {Count} bookings for user", Bookings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user's bookings");
                ErrorMessage = "An error occurred while loading your bookings. Please try again later.";
            }
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            try
            {
                _logger.LogInformation("Cancelling booking: {BookingId}", id);
                
                // TODO: Implement cancellation logic with the API
                // For now, we'll simulate cancellation success
                
                TempData["SuccessMessage"] = "Your booking has been cancelled successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking: {BookingId}", id);
                TempData["ErrorMessage"] = "An error occurred while cancelling your booking. Please try again.";
                return RedirectToPage();
            }
        }
    }
} 