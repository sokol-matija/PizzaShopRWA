using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    public interface ITripService
    {
        Task<List<Trip>> GetAllTripsAsync();
        Task<Trip?> GetTripByIdAsync(int id);
        Task<List<Trip>> GetTripsByDestinationAsync(int destinationId);
        Task<bool> CreateTripAsync(Trip trip);
        Task<bool> UpdateTripAsync(Trip trip);
        Task<bool> DeleteTripAsync(int id);
        Task<bool> AssignGuideToTripAsync(int tripId, int guideId);
        Task<bool> RemoveGuideFromTripAsync(int tripId, int guideId);
    }

    public class TripService : ITripService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<TripService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public TripService(IApiService apiService, ILogger<TripService> logger)
        {
            _apiService = apiService;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                MaxDepth = 64
            };
        }

        public async Task<List<Trip>> GetAllTripsAsync()
        {
            try
            {
                var response = await _apiService.GetAsync("Trip");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Trip>>(content, _jsonOptions) ?? new List<Trip>();
                }
                
                _logger.LogError($"Failed to get trips: {response.StatusCode}");
                return new List<Trip>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching trips");
                return new List<Trip>();
            }
        }

        public async Task<Trip?> GetTripByIdAsync(int id)
        {
            try
            {
                var response = await _apiService.GetAsync($"Trip/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Trip>(content, _jsonOptions);
                }
                
                _logger.LogError($"Failed to get trip with ID {id}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching trip with ID {id}");
                return null;
            }
        }

        public async Task<List<Trip>> GetTripsByDestinationAsync(int destinationId)
        {
            try
            {
                var response = await _apiService.GetAsync($"Trip/destination/{destinationId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Trip>>(content, _jsonOptions) ?? new List<Trip>();
                }
                
                _logger.LogError($"Failed to get trips for destination {destinationId}: {response.StatusCode}");
                return new List<Trip>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching trips for destination {destinationId}");
                return new List<Trip>();
            }
        }

        public async Task<bool> CreateTripAsync(Trip trip)
        {
            try
            {
                var response = await _apiService.PostAsync("Trip", trip);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating trip");
                return false;
            }
        }

        public async Task<bool> UpdateTripAsync(Trip trip)
        {
            try
            {
                var response = await _apiService.PutAsync($"Trip/{trip.Id}", trip);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating trip with ID {trip.Id}");
                return false;
            }
        }

        public async Task<bool> DeleteTripAsync(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"Trip/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting trip with ID {id}");
                return false;
            }
        }

        public async Task<bool> AssignGuideToTripAsync(int tripId, int guideId)
        {
            try
            {
                var response = await _apiService.PostAsync<object>($"Trip/{tripId}/guides/{guideId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while assigning guide {guideId} to trip {tripId}");
                return false;
            }
        }

        public async Task<bool> RemoveGuideFromTripAsync(int tripId, int guideId)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"Trip/{tripId}/guides/{guideId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while removing guide {guideId} from trip {tripId}");
                return false;
            }
        }
    }
} 