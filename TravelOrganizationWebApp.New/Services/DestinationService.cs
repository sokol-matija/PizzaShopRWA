using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TravelOrganizationWebApp.Models;
using Microsoft.Extensions.Logging;

namespace TravelOrganizationWebApp.Services
{
    /// <summary>
    /// Service for destination-related operations using the API
    /// </summary>
    public class DestinationService : IDestinationService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DestinationService> _logger;
        private readonly IUnsplashService _unsplashService;
        private readonly string _apiBaseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public DestinationService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<DestinationService> logger,
            IUnsplashService unsplashService,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _unsplashService = unsplashService;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:16000/api/";
            
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
        /// Get all available destinations
        /// </summary>
        public async Task<List<DestinationModel>> GetAllDestinationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Destination");
                if (response.IsSuccessStatusCode)
                {
                    var destinations = await response.Content.ReadFromJsonAsync<List<DestinationModel>>() ?? new List<DestinationModel>();
                    
                    // Get Unsplash images for destinations without an image URL and update them
                    foreach (var destination in destinations.Where(d => string.IsNullOrEmpty(d.ImageUrl)))
                    {
                        var searchQuery = $"{destination.City} {destination.Country} travel";
                        var imageUrl = await _unsplashService.GetRandomImageUrlAsync(searchQuery);
                        
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            destination.ImageUrl = imageUrl;
                            // Update the destination in the database with the new image URL
                            await UpdateDestinationImageAsync(destination.Id, imageUrl);
                        }
                    }
                    
                    return destinations;
                }
                else
                {
                    _logger.LogWarning("Failed to get destinations: {StatusCode}", response.StatusCode);
                    return new List<DestinationModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting destinations");
                return new List<DestinationModel>();
            }
        }

        private async Task<bool> UpdateDestinationImageAsync(int destinationId, string imageUrl)
        {
            try
            {
                await SetAuthHeaderAsync();
                var response = await _httpClient.PutAsync(
                    $"{_apiBaseUrl}Destination/{destinationId}/image",
                    new StringContent(JsonSerializer.Serialize(new { imageUrl }), Encoding.UTF8, "application/json"));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating destination image");
                return false;
            }
        }

        /// <summary>
        /// Get a specific destination by ID
        /// </summary>
        public async Task<DestinationModel?> GetDestinationByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Destination/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DestinationModel>();
                }
                else
                {
                    _logger.LogWarning("Failed to get destination {Id}: {StatusCode}", id, response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting destination {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// Create a new destination (admin only)
        /// </summary>
        public async Task<DestinationModel?> CreateDestinationAsync(DestinationModel destination)
        {
            try
            {
                // Set authentication token from cookie
                await SetAuthHeaderAsync();

                // Create the request content
                var content = new StringContent(
                    JsonSerializer.Serialize(destination),
                    Encoding.UTF8,
                    "application/json");

                // Make the API request
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}Destination", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var createdDestination = JsonSerializer.Deserialize<DestinationModel>(responseContent, _jsonOptions);
                    _logger.LogInformation("Successfully created destination: {Name}", destination.Name);
                    return createdDestination;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create destination: API returned {StatusCode} with message: {ErrorMessage}",
                        response.StatusCode, errorContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating destination");
                return null;
            }
        }

        /// <summary>
        /// Update an existing destination (admin only)
        /// </summary>
        public async Task<DestinationModel?> UpdateDestinationAsync(int id, DestinationModel destination)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var json = JsonSerializer.Serialize(destination);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}Destination/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<DestinationModel>(responseContent, _jsonOptions);
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
        /// Delete a destination (admin only)
        /// </summary>
        public async Task<bool> DeleteDestinationAsync(int id)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}Destination/{id}");
                
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