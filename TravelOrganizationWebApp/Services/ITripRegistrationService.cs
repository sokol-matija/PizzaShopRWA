using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    /// <summary>
    /// Interface for trip registration operations
    /// </summary>
    public interface ITripRegistrationService
    {
        /// <summary>
        /// Gets all trip registrations from the API
        /// </summary>
        Task<List<TripRegistration>> GetAllTripRegistrationsAsync();
        
        /// <summary>
        /// Gets all trip registrations for a specific user
        /// </summary>
        Task<List<TripRegistration>> GetUserRegistrationsAsync(string userId);
        
        /// <summary>
        /// Gets all registrations for a specific trip
        /// </summary>
        Task<List<TripRegistration>> GetTripRegistrationsAsync(int tripId);
        
        /// <summary>
        /// Gets a specific registration by ID
        /// </summary>
        Task<TripRegistration?> GetRegistrationByIdAsync(int id);
        
        /// <summary>
        /// Creates a new trip registration
        /// </summary>
        Task<bool> CreateRegistrationAsync(TripRegistration registration);
        
        /// <summary>
        /// Updates an existing registration
        /// </summary>
        Task<bool> UpdateRegistrationAsync(TripRegistration registration);
        
        /// <summary>
        /// Cancels a registration
        /// </summary>
        Task<bool> CancelRegistrationAsync(int id);
        
        /// <summary>
        /// Confirms a registration
        /// </summary>
        Task<bool> ConfirmRegistrationAsync(int id);
    }

    /// <summary>
    /// Implementation of the trip registration service that communicates with the API
    /// </summary>
    public class TripRegistrationService : ITripRegistrationService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<TripRegistrationService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the TripRegistrationService class
        /// </summary>
        public TripRegistrationService(IApiService apiService, ILogger<TripRegistrationService> logger)
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

        /// <summary>
        /// Gets all trip registrations from the API
        /// </summary>
        public async Task<List<TripRegistration>> GetAllTripRegistrationsAsync()
        {
            try
            {
                var response = await _apiService.GetAsync("TripRegistration");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TripRegistration>>(content, _jsonOptions) ?? new List<TripRegistration>();
                }
                
                _logger.LogError($"Failed to get trip registrations: {response.StatusCode}");
                return new List<TripRegistration>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching trip registrations");
                return new List<TripRegistration>();
            }
        }

        /// <summary>
        /// Gets all trip registrations for a specific user
        /// </summary>
        public async Task<List<TripRegistration>> GetUserRegistrationsAsync(string userId)
        {
            try
            {
                var response = await _apiService.GetAsync($"TripRegistration/user/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TripRegistration>>(content, _jsonOptions) ?? new List<TripRegistration>();
                }
                
                _logger.LogError($"Failed to get registrations for user {userId}: {response.StatusCode}");
                return new List<TripRegistration>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching registrations for user {userId}");
                return new List<TripRegistration>();
            }
        }

        /// <summary>
        /// Gets all registrations for a specific trip
        /// </summary>
        public async Task<List<TripRegistration>> GetTripRegistrationsAsync(int tripId)
        {
            try
            {
                var response = await _apiService.GetAsync($"TripRegistration/trip/{tripId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<TripRegistration>>(content, _jsonOptions) ?? new List<TripRegistration>();
                }
                
                _logger.LogError($"Failed to get registrations for trip {tripId}: {response.StatusCode}");
                return new List<TripRegistration>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching registrations for trip {tripId}");
                return new List<TripRegistration>();
            }
        }

        /// <summary>
        /// Gets a specific registration by ID
        /// </summary>
        public async Task<TripRegistration?> GetRegistrationByIdAsync(int id)
        {
            try
            {
                var response = await _apiService.GetAsync($"TripRegistration/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripRegistration>(content, _jsonOptions);
                }
                
                _logger.LogError($"Failed to get registration with ID {id}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching registration with ID {id}");
                return null;
            }
        }

        /// <summary>
        /// Creates a new trip registration
        /// </summary>
        public async Task<bool> CreateRegistrationAsync(TripRegistration registration)
        {
            try
            {
                var response = await _apiService.PostAsync("TripRegistration", registration);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating trip registration");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing registration
        /// </summary>
        public async Task<bool> UpdateRegistrationAsync(TripRegistration registration)
        {
            try
            {
                var response = await _apiService.PutAsync($"TripRegistration/{registration.Id}", registration);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating registration with ID {registration.Id}");
                return false;
            }
        }

        /// <summary>
        /// Cancels a registration
        /// </summary>
        public async Task<bool> CancelRegistrationAsync(int id)
        {
            try
            {
                var response = await _apiService.PutAsync<object>($"TripRegistration/{id}/cancel", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while cancelling registration with ID {id}");
                return false;
            }
        }

        /// <summary>
        /// Confirms a registration
        /// </summary>
        public async Task<bool> ConfirmRegistrationAsync(int id)
        {
            try
            {
                var response = await _apiService.PutAsync<object>($"TripRegistration/{id}/confirm", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while confirming registration with ID {id}");
                return false;
            }
        }
    }
} 