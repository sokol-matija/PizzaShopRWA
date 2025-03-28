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
    /// Page for booking a trip
    /// </summary>
    [Authorize] // Only authenticated users can book trips
    public class BookModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly ITripRegistrationService _registrationService;
        private readonly IAuthService _authService;
        private readonly ILogger<BookModel> _logger;

        public BookModel(
            ITripService tripService,
            ITripRegistrationService registrationService,
            IAuthService authService,
            ILogger<BookModel> logger)
        {
            _tripService = tripService;
            _registrationService = registrationService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// The trip being booked
        /// </summary>
        public TripModel? Trip { get; set; }

        /// <summary>
        /// Error message if API call fails
        /// </summary>
        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Success message after successful booking
        /// </summary>
        [TempData]
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Input model for the booking form
        /// </summary>
        [BindProperty]
        public BookingInputModel Input { get; set; } = new BookingInputModel();

        /// <summary>
        /// GET request handler to load trip details for booking
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
                
                // Check if trip is available for booking
                if (Trip.IsFull)
                {
                    ErrorMessage = "This trip is fully booked. Please select another trip.";
                    return RedirectToPage("./Details", new { id = id.Value });
                }
                
                // Populate input model with trip ID and set default participants to 1
                Input.TripId = Trip.Id;
                Input.Participants = 1;
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading trip details for booking, TripId: {TripId}", id);
                ErrorMessage = "An error occurred while loading trip details. Please try again later.";
                return RedirectToPage("./Index");
            }
        }

        /// <summary>
        /// POST request handler for form submission
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload trip details if validation fails
                if (Input.TripId > 0)
                {
                    Trip = await _tripService.GetTripByIdAsync(Input.TripId);
                }
                return Page();
            }

            try
            {
                // Get the current user
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    ErrorMessage = "You must be logged in to book a trip.";
                    return RedirectToPage("/Account/Login");
                }
                
                // Get trip details to check availability
                Trip = await _tripService.GetTripByIdAsync(Input.TripId);
                if (Trip == null)
                {
                    ErrorMessage = "The selected trip is not available.";
                    return RedirectToPage("./Index");
                }
                
                // Check if trip has enough available slots
                if (Trip.AvailableSlots < Input.Participants)
                {
                    ModelState.AddModelError(nameof(Input.Participants), 
                        $"Only {Trip.AvailableSlots} slots available for this trip.");
                    return Page();
                }
                
                // Create registration model
                var registration = new TripRegistrationModel
                {
                    TripId = Input.TripId,
                    UserId = currentUser.Id,
                    Participants = Input.Participants,
                    RegistrationDate = DateTime.UtcNow,
                    SpecialRequests = Input.SpecialRequests,
                    Status = "Pending"
                };
                
                // Submit booking
                var result = await _registrationService.CreateRegistrationAsync(registration);
                
                if (result != null)
                {
                    _logger.LogInformation("Trip booking successful: TripId: {TripId}, UserId: {UserId}, BookingId: {BookingId}", 
                        Input.TripId, currentUser.Id, result.Id);
                    
                    SuccessMessage = "Your trip booking was successful! You can view your booking details under 'My Bookings'.";
                    return RedirectToPage("/MyBookings/Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Unable to complete your booking. Please try again later.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting trip booking, TripId: {TripId}", Input.TripId);
                ModelState.AddModelError(string.Empty, "An error occurred while processing your booking.");
                
                // Reload trip details
                if (Input.TripId > 0)
                {
                    Trip = await _tripService.GetTripByIdAsync(Input.TripId);
                }
                
                return Page();
            }
        }

        /// <summary>
        /// Input model for the booking form
        /// </summary>
        public class BookingInputModel
        {
            public int TripId { get; set; }
            
            [Required(ErrorMessage = "Number of participants is required")]
            [Range(1, 20, ErrorMessage = "Number of participants must be between 1 and 20")]
            [Display(Name = "Number of Participants")]
            public int Participants { get; set; } = 1;
            
            [Display(Name = "Special Requests")]
            [StringLength(500, ErrorMessage = "Special requests cannot exceed 500 characters")]
            public string? SpecialRequests { get; set; }
        }
    }
} 