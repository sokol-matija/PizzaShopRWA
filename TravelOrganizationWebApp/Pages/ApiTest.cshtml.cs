using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Pages
{
    public class ApiTestModel : PageModel
    {
        private readonly IDestinationService _destinationService;
        private readonly ITripService _tripService;
        private readonly IGuideService _guideService;
        private readonly ITripRegistrationService _tripRegistrationService;
        private readonly ILogger<ApiTestModel> _logger;

        public List<Destination> Destinations { get; set; } = new List<Destination>();
        public List<Trip> Trips { get; set; } = new List<Trip>();
        public List<Guide> Guides { get; set; } = new List<Guide>();
        public List<TripRegistration> TripRegistrations { get; set; } = new List<TripRegistration>();
        
        public string ErrorMessage { get; set; } = string.Empty;
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ApiTestModel(
            IDestinationService destinationService,
            ITripService tripService,
            IGuideService guideService,
            ITripRegistrationService tripRegistrationService,
            ILogger<ApiTestModel> logger)
        {
            _destinationService = destinationService;
            _tripService = tripService;
            _guideService = guideService;
            _tripRegistrationService = tripRegistrationService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // Test destination service
                Destinations = await _destinationService.GetAllDestinationsAsync();
                _logger.LogInformation($"Retrieved {Destinations.Count} destinations");

                // Test trip service
                Trips = await _tripService.GetAllTripsAsync();
                _logger.LogInformation($"Retrieved {Trips.Count} trips");

                // Test guide service
                Guides = await _guideService.GetAllGuidesAsync();
                _logger.LogInformation($"Retrieved {Guides.Count} guides");

                // Test trip registration service
                TripRegistrations = await _tripRegistrationService.GetAllTripRegistrationsAsync();
                _logger.LogInformation($"Retrieved {TripRegistrations.Count} trip registrations");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error occurred while testing API services: {ex.Message}";
                _logger.LogError(ex, "Error occurred during API testing");
            }
        }
    }
} 