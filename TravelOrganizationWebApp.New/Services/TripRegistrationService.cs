using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    /// <summary>
    /// Service for trip registration (booking) operations using the API
    /// </summary>
    public class TripRegistrationService : ITripRegistrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<TripRegistrationService> _logger;

        public TripRegistrationService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<TripRegistrationService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
            
            // Configure base address from settings
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"] ?? 
                throw new InvalidOperationException("API BaseUrl not configured"));
            
            // Configure JSON options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }
        
        /// <summary>
        /// Set authentication token for API requests if user is logged in
        /// </summary>
        private async Task SetAuthHeaderAsync()
        {
            // Clear any existing Authorization headers
            _httpClient.DefaultRequestHeaders.Authorization = null;
            
            // Get the current HTTP context
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;
            
            // Check if user is authenticated
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                // Get the token from the authentication cookie
                var token = await httpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
                if (!string.IsNullOrEmpty(token))
                {
                    // Add the token to request headers
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        /// <summary>
        /// Get all registrations (admin only)
        /// </summary>
        public async Task<List<TripRegistrationModel>> GetAllRegistrationsAsync()
        {
            try
            {
                // Set auth header
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.GetAsync("TripRegistration");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var registrations = JsonSerializer.Deserialize<List<TripRegistrationModel>>(content, _jsonOptions);
                    return registrations ?? new List<TripRegistrationModel>();
                }
                
                _logger.LogWarning("Failed to get all registrations: {StatusCode}", response.StatusCode);
                return new List<TripRegistrationModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all registrations");
                return new List<TripRegistrationModel>();
            }
        }

        /// <summary>
        /// Get a specific registration by ID
        /// </summary>
        public async Task<TripRegistrationModel?> GetRegistrationByIdAsync(int id)
        {
            try
            {
                // Set auth header
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.GetAsync($"TripRegistration/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripRegistrationModel>(content, _jsonOptions);
                }
                
                _logger.LogWarning("Failed to get registration {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registration {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Get all registrations for a specific user
        /// </summary>
        public async Task<List<TripRegistrationModel>> GetRegistrationsByUserAsync(int userId)
        {
            try
            {
                // Set auth header
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.GetAsync($"TripRegistration/user/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var registrations = JsonSerializer.Deserialize<List<TripRegistrationModel>>(content, _jsonOptions);
                    return registrations ?? new List<TripRegistrationModel>();
                }
                
                _logger.LogWarning("Failed to get registrations for user {UserId}: {StatusCode}", userId, response.StatusCode);
                return new List<TripRegistrationModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registrations for user {UserId}", userId);
                return new List<TripRegistrationModel>();
            }
        }

        /// <summary>
        /// Get all registrations for a specific trip (admin only)
        /// </summary>
        public async Task<List<TripRegistrationModel>> GetRegistrationsByTripAsync(int tripId)
        {
            try
            {
                // Set auth header
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.GetAsync($"TripRegistration/trip/{tripId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var registrations = JsonSerializer.Deserialize<List<TripRegistrationModel>>(content, _jsonOptions);
                    return registrations ?? new List<TripRegistrationModel>();
                }
                
                _logger.LogWarning("Failed to get registrations for trip {TripId}: {StatusCode}", tripId, response.StatusCode);
                return new List<TripRegistrationModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting registrations for trip {TripId}", tripId);
                return new List<TripRegistrationModel>();
            }
        }

        /// <summary>
        /// Create a new registration (book a trip)
        /// </summary>
        public async Task<TripRegistrationModel?> CreateRegistrationAsync(TripRegistrationModel registration)
        {
            try
            {
                // Set auth header
                await SetAuthHeaderAsync();
                
                var json = JsonSerializer.Serialize(registration);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("TripRegistration", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripRegistrationModel>(responseContent, _jsonOptions);
                }
                
                _logger.LogWarning("Failed to create registration: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating registration");
                return null;
            }
        }

        /// <summary>
        /// Update an existing registration
        /// </summary>
        public async Task<TripRegistrationModel?> UpdateRegistrationAsync(int id, TripRegistrationModel registration)
        {
            try
            {
                // Set auth header
                await SetAuthHeaderAsync();
                
                var json = JsonSerializer.Serialize(registration);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"TripRegistration/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripRegistrationModel>(responseContent, _jsonOptions);
                }
                
                _logger.LogWarning("Failed to update registration {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating registration {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Delete a registration (cancel a booking)
        /// </summary>
        public async Task<bool> DeleteRegistrationAsync(int id)
        {
            try
            {
                // Set auth header
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.DeleteAsync($"TripRegistration/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                _logger.LogWarning("Failed to delete registration {Id}: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting registration {Id}", id);
                return false;
            }
        }

        /// <summary>
        /// Update the status of a registration (admin only)
        /// </summary>
        public async Task<bool> UpdateRegistrationStatusAsync(int id, string status)
        {
            try
            {
                // Set auth header
                await SetAuthHeaderAsync();
                
                var content = new StringContent($"\"{status}\"", Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PatchAsync($"TripRegistration/{id}/status", content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                _logger.LogWarning("Failed to update registration {Id} status: {StatusCode}", id, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating registration {Id} status", id);
                return false;
            }
        }
    }
} 