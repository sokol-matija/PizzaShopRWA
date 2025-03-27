using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    /// <summary>
    /// Interface for guide-related operations
    /// </summary>
    public interface IGuideService
    {
        /// <summary>
        /// Gets all guides from the API
        /// </summary>
        Task<List<Guide>> GetAllGuidesAsync();
        
        /// <summary>
        /// Gets a specific guide by ID
        /// </summary>
        Task<Guide?> GetGuideByIdAsync(int id);
        
        /// <summary>
        /// Gets all guides associated with a specific trip
        /// </summary>
        Task<List<Guide>> GetGuidesByTripAsync(int tripId);
        
        /// <summary>
        /// Creates a new guide in the system
        /// </summary>
        Task<bool> CreateGuideAsync(Guide guide);
        
        /// <summary>
        /// Updates an existing guide's information
        /// </summary>
        Task<bool> UpdateGuideAsync(Guide guide);
        
        /// <summary>
        /// Deletes a guide from the system
        /// </summary>
        Task<bool> DeleteGuideAsync(int id);
    }

    /// <summary>
    /// Implementation of the guide service that communicates with the API
    /// </summary>
    public class GuideService : IGuideService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<GuideService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the GuideService class
        /// </summary>
        public GuideService(IApiService apiService, ILogger<GuideService> logger)
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
        /// Gets all guides from the API
        /// </summary>
        public async Task<List<Guide>> GetAllGuidesAsync()
        {
            try
            {
                var response = await _apiService.GetAsync("Guide");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Guide>>(content, _jsonOptions) ?? new List<Guide>();
                }
                
                _logger.LogError($"Failed to get guides: {response.StatusCode}");
                return new List<Guide>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching guides");
                return new List<Guide>();
            }
        }

        /// <summary>
        /// Gets a specific guide by ID
        /// </summary>
        public async Task<Guide?> GetGuideByIdAsync(int id)
        {
            try
            {
                var response = await _apiService.GetAsync($"Guide/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Guide>(content, _jsonOptions);
                }
                
                _logger.LogError($"Failed to get guide with ID {id}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching guide with ID {id}");
                return null;
            }
        }

        /// <summary>
        /// Gets all guides associated with a specific trip
        /// </summary>
        public async Task<List<Guide>> GetGuidesByTripAsync(int tripId)
        {
            try
            {
                var response = await _apiService.GetAsync($"Trip/{tripId}/guides");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Guide>>(content, _jsonOptions) ?? new List<Guide>();
                }
                
                _logger.LogError($"Failed to get guides for trip {tripId}: {response.StatusCode}");
                return new List<Guide>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching guides for trip {tripId}");
                return new List<Guide>();
            }
        }

        /// <summary>
        /// Creates a new guide in the system
        /// </summary>
        public async Task<bool> CreateGuideAsync(Guide guide)
        {
            try
            {
                var response = await _apiService.PostAsync("Guide", guide);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating guide");
                return false;
            }
        }

        /// <summary>
        /// Updates an existing guide's information
        /// </summary>
        public async Task<bool> UpdateGuideAsync(Guide guide)
        {
            try
            {
                var response = await _apiService.PutAsync($"Guide/{guide.Id}", guide);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating guide with ID {guide.Id}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a guide from the system
        /// </summary>
        public async Task<bool> DeleteGuideAsync(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"Guide/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting guide with ID {id}");
                return false;
            }
        }
    }
} 