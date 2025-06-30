using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Service for managing travel guides through API calls
    /// Handles name field mapping between API (single Name) and WebApp (FirstName/LastName)
    /// </summary>
    public class GuideService : IGuideService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GuideService> _logger;
        private readonly string _apiBaseUrl;

        public GuideService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ILogger<GuideService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:16000/api/";
            
            // Ensure base URL ends with slash
            if (!_apiBaseUrl.EndsWith("/"))
                _apiBaseUrl += "/";
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
            
            // First try to get token from session (like AuthService does)
            var sessionToken = httpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(sessionToken))
            {
                _logger.LogInformation("Using token from session for Guide API request - Token: {TokenPreview}...", 
                    sessionToken.Substring(0, Math.Min(20, sessionToken.Length)));
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
                return;
            }
            
            // If no session token, try from authentication cookie
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                // Get the token from the authentication cookie
                var cookieToken = await httpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
                if (!string.IsNullOrEmpty(cookieToken))
                {
                    _logger.LogInformation("Using token from authentication cookie for Guide API request");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cookieToken);
                    return;
                }
            }
            
            _logger.LogWarning("No authentication token found in session or cookie for Guide API request");
        }

        /// <summary>
        /// Get all available guides
        /// </summary>
        public async Task<IEnumerable<GuideModel>> GetAllGuidesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all guides from API");
                
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Guide");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response: {JsonContent}", jsonContent.Substring(0, Math.Min(200, jsonContent.Length)));
                    
                    // Handle the $values wrapper from .NET serialization
                    var apiGuides = DeserializeApiResponse<IEnumerable<ApiGuideModel>>(jsonContent);

                    var guides = apiGuides?.Select(MapFromApiModel) ?? Enumerable.Empty<GuideModel>();
                    
                    _logger.LogInformation("Successfully loaded {Count} guides", guides.Count());
                    return guides;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch guides: {StatusCode} - {ReasonPhrase}", 
                        response.StatusCode, response.ReasonPhrase);
                    return Enumerable.Empty<GuideModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching guides");
                return Enumerable.Empty<GuideModel>();
            }
        }

        /// <summary>
        /// Get a specific guide by ID
        /// </summary>
        public async Task<GuideModel?> GetGuideByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching guide with ID: {GuideId}", id);
                
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Guide/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiGuide = DeserializeApiResponse<ApiGuideModel>(jsonContent);

                    if (apiGuide != null)
                    {
                        var guide = MapFromApiModel(apiGuide);
                        _logger.LogInformation("Successfully loaded guide: {GuideName}", guide.FullName);
                        return guide;
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Guide with ID {GuideId} not found", id);
                }
                else
                {
                    _logger.LogWarning("Failed to fetch guide {GuideId}: {StatusCode} - {ReasonPhrase}", 
                        id, response.StatusCode, response.ReasonPhrase);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching guide {GuideId}", id);
                return null;
            }
        }

        /// <summary>
        /// Create a new guide
        /// </summary>
        public async Task<GuideModel?> CreateGuideAsync(GuideModel guide)
        {
            try
            {
                _logger.LogInformation("Creating new guide: {GuideName}", guide.FullName);
                
                // Set authentication token for Admin-required operation
                await SetAuthHeaderAsync();
                
                var apiGuide = MapToApiModel(guide);
                var jsonContent = JsonSerializer.Serialize(apiGuide);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}Guide", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var createdApiGuide = DeserializeApiResponse<ApiGuideModel>(responseContent);

                    if (createdApiGuide != null)
                    {
                        var createdGuide = MapFromApiModel(createdApiGuide);
                        _logger.LogInformation("Successfully created guide with ID: {GuideId}", createdGuide.Id);
                        return createdGuide;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to create guide: {StatusCode} - {Error}. Auth header: {AuthHeader}", 
                        response.StatusCode, errorContent, 
                        _httpClient.DefaultRequestHeaders.Authorization?.ToString() ?? "None");
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating guide");
                return null;
            }
        }

        /// <summary>
        /// Update an existing guide
        /// </summary>
        public async Task<GuideModel?> UpdateGuideAsync(int id, GuideModel guide)
        {
            try
            {
                _logger.LogInformation("Updating guide with ID: {GuideId}", id);
                
                // Set authentication token for Admin-required operation
                await SetAuthHeaderAsync();
                
                var apiGuide = MapToApiModel(guide);
                apiGuide.Id = id; // Ensure ID is set correctly
                
                var jsonContent = JsonSerializer.Serialize(apiGuide);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}Guide/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var updatedApiGuide = DeserializeApiResponse<ApiGuideModel>(responseContent);

                    if (updatedApiGuide != null)
                    {
                        var updatedGuide = MapFromApiModel(updatedApiGuide);
                        _logger.LogInformation("Successfully updated guide: {GuideName}", updatedGuide.FullName);
                        return updatedGuide;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to update guide {GuideId}: {StatusCode} - {Error}", 
                        id, response.StatusCode, errorContent);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating guide {GuideId}", id);
                return null;
            }
        }

        /// <summary>
        /// Delete a guide
        /// </summary>
        public async Task<bool> DeleteGuideAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting guide with ID: {GuideId}", id);
                
                // Set authentication token for Admin-required operation
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}Guide/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully deleted guide with ID: {GuideId}", id);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to delete guide {GuideId}: {StatusCode} - {Error}", 
                        id, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting guide {GuideId}", id);
                return false;
            }
        }

        /// <summary>
        /// Get all guides assigned to a specific trip
        /// </summary>
        public async Task<IEnumerable<GuideModel>> GetGuidesByTripAsync(int tripId)
        {
            try
            {
                _logger.LogInformation("Fetching guides for trip ID: {TripId}", tripId);
                
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Guide/trip/{tripId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiGuides = DeserializeApiResponse<IEnumerable<ApiGuideModel>>(jsonContent);

                    var guides = apiGuides?.Select(MapFromApiModel) ?? Enumerable.Empty<GuideModel>();
                    
                    _logger.LogInformation("Successfully loaded {Count} guides for trip {TripId}", guides.Count(), tripId);
                    return guides;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch guides for trip {TripId}: {StatusCode} - {ReasonPhrase}", 
                        tripId, response.StatusCode, response.ReasonPhrase);
                    return Enumerable.Empty<GuideModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching guides for trip {TripId}", tripId);
                return Enumerable.Empty<GuideModel>();
            }
        }

        /// <summary>
        /// Map from API model (single Name field) to WebApp model (FirstName/LastName)
        /// </summary>
        private static GuideModel MapFromApiModel(ApiGuideModel apiGuide)
        {
            var nameParts = SplitName(apiGuide.Name ?? "");
            
            return new GuideModel
            {
                Id = apiGuide.Id,
                FirstName = nameParts.firstName,
                LastName = nameParts.lastName,
                Bio = apiGuide.Bio,
                Email = apiGuide.Email,
                PhoneNumber = apiGuide.Phone,
                PhotoUrl = apiGuide.ImageUrl,
                YearsExperience = apiGuide.YearsOfExperience
            };
        }

        /// <summary>
        /// Map from WebApp model (FirstName/LastName) to API model (single Name field)
        /// </summary>
        private static ApiGuideModel MapToApiModel(GuideModel guide)
        {
            return new ApiGuideModel
            {
                Id = guide.Id,
                Name = $"{guide.FirstName?.Trim()} {guide.LastName?.Trim()}".Trim(),
                Bio = guide.Bio,
                Email = guide.Email,
                Phone = guide.PhoneNumber,
                ImageUrl = guide.PhotoUrl,
                YearsOfExperience = guide.YearsExperience
            };
        }

        /// <summary>
        /// Split a full name into first and last name components
        /// Handles various name formats gracefully
        /// </summary>
        private static (string firstName, string lastName) SplitName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return ("", "");

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            return parts.Length switch
            {
                0 => ("", ""),
                1 => (parts[0], ""),
                _ => (parts[0], string.Join(" ", parts.Skip(1)))
            };
        }

        /// <summary>
        /// Deserialize API response handling $values wrapper
        /// </summary>
        private static T? DeserializeApiResponse<T>(string jsonContent)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                // First try direct deserialization
                return JsonSerializer.Deserialize<T>(jsonContent, options);
            }
            catch
            {
                try
                {
                    // If that fails, try parsing as wrapped response with $values
                    using var document = JsonDocument.Parse(jsonContent);
                    var root = document.RootElement;
                    
                    if (root.TryGetProperty("$values", out var valuesProperty))
                    {
                        var valuesJson = valuesProperty.GetRawText();
                        return JsonSerializer.Deserialize<T>(valuesJson, options);
                    }
                    
                    return default(T);
                }
                catch
                {
                    return default(T);
                }
            }
        }

        /// <summary>
        /// API model that matches the backend Guide entity structure
        /// </summary>
        private class ApiGuideModel
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public string? Bio { get; set; }
            public string? Email { get; set; }
            public string? Phone { get; set; }
            public string? ImageUrl { get; set; }
            public int? YearsOfExperience { get; set; }
        }
    }
} 