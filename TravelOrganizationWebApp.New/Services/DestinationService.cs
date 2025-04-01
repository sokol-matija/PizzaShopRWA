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
            
            // Configure base address from settings
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? 
                "http://localhost:16000/api/");
                
            // Set API base URL to empty as BaseAddress already has the api/ prefix
            _apiBaseUrl = "";
            
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
        /// Get all destinations
        /// </summary>
        public async Task<List<DestinationModel>> GetAllDestinationsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all destinations from API");
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Destination");
                
                if (response.IsSuccessStatusCode)
                {
                    // Read response content as string first to handle the reference-preserving format
                    var content = await response.Content.ReadAsStringAsync();
                    
                    // Configure JSON options to handle reference preservation
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                    };
                    
                    List<DestinationModel> destinations;
                    
                    try {
                        // Try to parse directly first
                        destinations = JsonSerializer.Deserialize<List<DestinationModel>>(content, jsonOptions);
                        if (destinations == null)
                        {
                            destinations = new List<DestinationModel>();
                        }
                    }
                    catch
                    {
                        // If direct parsing fails, try to extract the $values array from reference-preserving format
                        try
                        {
                            var responseObj = JsonSerializer.Deserialize<JsonDocument>(content, jsonOptions);
                            // Check if the root has a $values property (reference-preserving format)
                            if (responseObj?.RootElement.TryGetProperty("$values", out var valuesElement) == true)
                            {
                                destinations = JsonSerializer.Deserialize<List<DestinationModel>>(
                                    valuesElement.GetRawText(), jsonOptions) ?? new List<DestinationModel>();
                            }
                            else
                            {
                                // Fallback if structure is different
                                destinations = new List<DestinationModel>();
                                _logger.LogWarning("Unexpected JSON format from API");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to parse destination data");
                            destinations = new List<DestinationModel>();
                        }
                    }
                    
                    // Get Unsplash images for destinations without an image URL and update them
                    foreach (var destination in destinations.Where(d => string.IsNullOrEmpty(d.ImageUrl)))
                    {
                        try
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
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error getting Unsplash image for {City}, {Country}", 
                                destination.City, destination.Country);
                            // Continue processing other destinations
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
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("Destination API response: {Content}", content);
                    
                    var destination = await response.Content.ReadFromJsonAsync<DestinationModel>();
                    
                    if (destination != null)
                    {
                        // Add well-known taglines for famous destinations
                        destination.Tagline = GetDestinationTagline(destination);
                    
                        // If the destination doesn't have an image URL, try to get one from Unsplash
                        if (string.IsNullOrEmpty(destination.ImageUrl))
                        {
                            try
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
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error getting Unsplash image for destination {Id}", id);
                                // Continue processing
                            }
                        }
                    }
                    
                    return destination;
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
        /// Returns a well-known tagline for a destination based on city and country
        /// </summary>
        private string? GetDestinationTagline(DestinationModel destination)
        {
            // Match based on city name (case insensitive)
            return (destination.City?.ToLowerInvariant(), destination.Country?.ToLowerInvariant()) switch
            {
                ("paris", "france") => "The City of Light",
                ("new york", "united states") => "The Big Apple",
                ("rome", "italy") => "The Eternal City",
                ("venice", "italy") => "The Floating City",
                ("barcelona", "spain") => "The City of Gaudi",
                ("london", "united kingdom") => "The Big Smoke",
                ("tokyo", "japan") => "The Eastern Capital",
                ("dubai", "united arab emirates") => "City of Gold",
                ("rio de janeiro", "brazil") => "Marvelous City",
                ("las vegas", "united states") => "Sin City",
                _ => null // No tagline for other destinations
            };
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