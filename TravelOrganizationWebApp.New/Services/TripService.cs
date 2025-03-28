using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    /// <summary>
    /// Service for trip-related operations using the API
    /// </summary>
    public class TripService : ITripService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;

        public TripService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            
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
        /// Get all available trips
        /// </summary>
        public async Task<List<TripModel>> GetAllTripsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("Trip");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var trips = JsonSerializer.Deserialize<List<TripModel>>(content, _jsonOptions);
                    return trips ?? new List<TripModel>();
                }
                
                // Handle errors
                return new List<TripModel>();
            }
            catch (Exception)
            {
                // Log exception in a real application
                return new List<TripModel>();
            }
        }

        /// <summary>
        /// Get a specific trip by ID
        /// </summary>
        public async Task<TripModel?> GetTripByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Trip/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripModel>(content, _jsonOptions);
                }
                
                // Handle errors
                return null;
            }
            catch (Exception)
            {
                // Log exception in a real application
                return null;
            }
        }

        /// <summary>
        /// Get all trips for a specific destination
        /// </summary>
        public async Task<List<TripModel>> GetTripsByDestinationAsync(int destinationId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Trip/destination/{destinationId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var trips = JsonSerializer.Deserialize<List<TripModel>>(content, _jsonOptions);
                    return trips ?? new List<TripModel>();
                }
                
                // Handle errors
                return new List<TripModel>();
            }
            catch (Exception)
            {
                // Log exception in a real application
                return new List<TripModel>();
            }
        }

        /// <summary>
        /// Create a new trip (admin only)
        /// </summary>
        public async Task<TripModel?> CreateTripAsync(TripModel trip)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var json = JsonSerializer.Serialize(trip);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("Trip", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripModel>(responseContent, _jsonOptions);
                }
                
                // Handle errors
                return null;
            }
            catch (Exception)
            {
                // Log exception in a real application
                return null;
            }
        }

        /// <summary>
        /// Update an existing trip (admin only)
        /// </summary>
        public async Task<TripModel?> UpdateTripAsync(int id, TripModel trip)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var json = JsonSerializer.Serialize(trip);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"Trip/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripModel>(responseContent, _jsonOptions);
                }
                
                // Handle errors
                return null;
            }
            catch (Exception)
            {
                // Log exception in a real application
                return null;
            }
        }

        /// <summary>
        /// Delete a trip (admin only)
        /// </summary>
        public async Task<bool> DeleteTripAsync(int id)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.DeleteAsync($"Trip/{id}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Log exception in a real application
                return false;
            }
        }

        /// <summary>
        /// Assign a guide to a trip (admin only)
        /// </summary>
        public async Task<bool> AssignGuideToTripAsync(int tripId, int guideId)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.PostAsync($"Trip/{tripId}/guides/{guideId}", null);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Log exception in a real application
                return false;
            }
        }

        /// <summary>
        /// Remove a guide from a trip (admin only)
        /// </summary>
        public async Task<bool> RemoveGuideFromTripAsync(int tripId, int guideId)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.DeleteAsync($"Trip/{tripId}/guides/{guideId}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                // Log exception in a real application
                return false;
            }
        }
    }
} 