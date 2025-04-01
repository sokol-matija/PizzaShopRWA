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
        private readonly string _apiBaseUrl;
        private readonly ILogger<TripService> _logger;
        private readonly IDestinationService _destinationService;

        public TripService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<TripService> logger,
            IDestinationService destinationService)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
            _destinationService = destinationService;
            
            // Configure base address from settings
            _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"] ?? 
                throw new InvalidOperationException("API BaseUrl not configured"));
            
            // Set API base URL
            _apiBaseUrl = ""; // No additional prefix
            
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
                // Log the request
                _logger.LogInformation("Fetching all trips from API");
                
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Trip");
                
                if (response.IsSuccessStatusCode)
                {
                    // Read response content as string first to handle the reference-preserving format
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Response for all trips: {Content}", content);
                    
                    // Configure JSON options to handle reference preservation
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                    };
                    
                    var trips = new List<TripModel>();
                    
                    // Parse JSON document to manually extract properties
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(content);
                        
                        // Handle different response formats ($values array or direct array)
                        JsonElement tripsArray;
                        if (jsonDoc.RootElement.TryGetProperty("$values", out var valuesElement))
                        {
                            tripsArray = valuesElement;
                        }
                        else if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            tripsArray = jsonDoc.RootElement;
                        }
                        else
                        {
                            _logger.LogWarning("Unexpected JSON structure in GetAllTripsAsync");
                            return new List<TripModel>();
                        }
                        
                        // Process each trip in the array
                        foreach (var tripElement in tripsArray.EnumerateArray())
                        {
                            var trip = new TripModel
                            {
                                Id = GetIntProperty(tripElement, "id"),
                                Title = GetStringProperty(tripElement, "name") ?? string.Empty,
                                Description = GetStringProperty(tripElement, "description") ?? string.Empty,
                                StartDate = GetDateTimeProperty(tripElement, "startDate"),
                                EndDate = GetDateTimeProperty(tripElement, "endDate"),
                                Price = GetDecimalProperty(tripElement, "price"),
                                ImageUrl = GetStringProperty(tripElement, "imageUrl"),
                                DestinationId = GetIntProperty(tripElement, "destinationId"),
                                DestinationName = GetStringProperty(tripElement, "destinationName"),
                                // Map capacity and bookings from API
                                Capacity = GetIntProperty(tripElement, "maxParticipants"),
                                CurrentBookings = GetIntProperty(tripElement, "maxParticipants") - GetIntProperty(tripElement, "availableSpots")
                            };
                            
                            // Process guides if present
                            if (tripElement.TryGetProperty("guides", out var guidesElement) && 
                                guidesElement.TryGetProperty("$values", out var guidesValues))
                            {
                                trip.Guides = new List<GuideModel>();
                                
                                foreach (var guideElement in guidesValues.EnumerateArray())
                                {
                                    var guide = new GuideModel
                                    {
                                        Id = GetIntProperty(guideElement, "id"),
                                        // Split name into first and last name
                                        FirstName = SplitName(GetStringProperty(guideElement, "name")).firstName,
                                        LastName = SplitName(GetStringProperty(guideElement, "name")).lastName,
                                        Bio = GetStringProperty(guideElement, "bio"),
                                        Email = GetStringProperty(guideElement, "email"),
                                        PhoneNumber = GetStringProperty(guideElement, "phone"),
                                        PhotoUrl = GetStringProperty(guideElement, "imageUrl"),
                                        YearsExperience = GetIntProperty(guideElement, "yearsOfExperience")
                                    };
                                    
                                    trip.Guides.Add(guide);
                                }
                            }
                            
                            trips.Add(trip);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error manually parsing trips JSON, falling back to automatic deserialization");
                        
                        // Fallback to old method if parsing fails
                        try
                        {
                            trips = JsonSerializer.Deserialize<List<TripModel>>(content, jsonOptions) ?? new List<TripModel>();
                        }
                        catch
                        {
                            try
                            {
                                var jsonDoc = JsonDocument.Parse(content);
                                if (jsonDoc.RootElement.TryGetProperty("$values", out var valuesElement))
                                {
                                    trips = JsonSerializer.Deserialize<List<TripModel>>(valuesElement.GetRawText(), jsonOptions) ?? new List<TripModel>();
                                }
                            }
                            catch (Exception innerEx)
                            {
                                _logger.LogError(innerEx, "All parsing methods failed for trips JSON");
                                return new List<TripModel>();
                            }
                        }
                    }
                    
                    // Enrich trips with destination images if needed
                    await EnrichTripsWithDestinationImagesAsync(trips);
                    
                    return trips;
                }
                else
                {
                    // Log the error
                    _logger.LogWarning("API Error: {StatusCode} - {ErrorContent}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                }
                
                // Handle errors
                return new List<TripModel>();
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in GetAllTripsAsync");
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
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Trip/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Response for trip {TripId}: {Content}", id, content);
                    
                    // Configure JSON options to handle reference preservation
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                    };
                    
                    try
                    {
                        // Parse JSON document to manually extract properties
                        var jsonDoc = JsonDocument.Parse(content);
                        var rootElement = jsonDoc.RootElement;
                        
                        // Create and populate a TripModel with values from the API
                        var trip = new TripModel
                        {
                            Id = GetIntProperty(rootElement, "id"),
                            Title = GetStringProperty(rootElement, "name") ?? string.Empty,
                            Description = GetStringProperty(rootElement, "description") ?? string.Empty,
                            StartDate = GetDateTimeProperty(rootElement, "startDate"),
                            EndDate = GetDateTimeProperty(rootElement, "endDate"),
                            Price = GetDecimalProperty(rootElement, "price"),
                            ImageUrl = GetStringProperty(rootElement, "imageUrl"),
                            DestinationId = GetIntProperty(rootElement, "destinationId"),
                            DestinationName = GetStringProperty(rootElement, "destinationName"),
                            // Map capacity and bookings from API
                            Capacity = GetIntProperty(rootElement, "maxParticipants"),
                            CurrentBookings = GetIntProperty(rootElement, "maxParticipants") - GetIntProperty(rootElement, "availableSpots")
                        };
                        
                        // Process guides if present
                        if (rootElement.TryGetProperty("guides", out var guidesElement) && guidesElement.TryGetProperty("$values", out var guidesValues))
                        {
                            trip.Guides = new List<GuideModel>();
                            
                            foreach (var guideElement in guidesValues.EnumerateArray())
                            {
                                var guide = new GuideModel
                                {
                                    Id = GetIntProperty(guideElement, "id"),
                                    // Split name into first and last name
                                    FirstName = SplitName(GetStringProperty(guideElement, "name")).firstName,
                                    LastName = SplitName(GetStringProperty(guideElement, "name")).lastName,
                                    Bio = GetStringProperty(guideElement, "bio"),
                                    Email = GetStringProperty(guideElement, "email"),
                                    PhoneNumber = GetStringProperty(guideElement, "phone"),
                                    PhotoUrl = GetStringProperty(guideElement, "imageUrl"),
                                    YearsExperience = GetIntProperty(guideElement, "yearsOfExperience")
                                };
                                
                                trip.Guides.Add(guide);
                            }
                        }
                        
                        return trip;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to parse trip data for ID {TripId}", id);
                        
                        // Fall back to standard deserialization as backup
                        try
                        {
                            return JsonSerializer.Deserialize<TripModel>(content, jsonOptions);
                        }
                        catch
                        {
                            _logger.LogError("Both parsing methods failed for trip ID {TripId}", id);
                            return null;
                        }
                    }
                }
                
                // Handle errors
                _logger.LogWarning("Failed to get trip {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in GetTripByIdAsync: {Id}", id);
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
                // Log the request
                _logger.LogInformation("Fetching trips for destination {DestinationId}", destinationId);
                
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Trip/destination/{destinationId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Response for destination {DestinationId}: {Content}", destinationId, content);
                    
                    // Configure JSON options to handle reference preservation
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                    };
                    
                    var trips = new List<TripModel>();
                    
                    // Parse JSON document to manually extract properties
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(content);
                        
                        // Handle different response formats ($values array or direct array)
                        JsonElement tripsArray;
                        if (jsonDoc.RootElement.TryGetProperty("$values", out var valuesElement))
                        {
                            tripsArray = valuesElement;
                        }
                        else if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                        {
                            tripsArray = jsonDoc.RootElement;
                        }
                        else
                        {
                            _logger.LogWarning("Unexpected JSON structure in GetTripsByDestinationAsync");
                            return new List<TripModel>();
                        }
                        
                        // Process each trip in the array
                        foreach (var tripElement in tripsArray.EnumerateArray())
                        {
                            var trip = new TripModel
                            {
                                Id = GetIntProperty(tripElement, "id"),
                                Title = GetStringProperty(tripElement, "name") ?? string.Empty,
                                Description = GetStringProperty(tripElement, "description") ?? string.Empty,
                                StartDate = GetDateTimeProperty(tripElement, "startDate"),
                                EndDate = GetDateTimeProperty(tripElement, "endDate"),
                                Price = GetDecimalProperty(tripElement, "price"),
                                ImageUrl = GetStringProperty(tripElement, "imageUrl"),
                                DestinationId = GetIntProperty(tripElement, "destinationId"),
                                DestinationName = GetStringProperty(tripElement, "destinationName"),
                                // Map capacity and bookings from API
                                Capacity = GetIntProperty(tripElement, "maxParticipants"),
                                CurrentBookings = GetIntProperty(tripElement, "maxParticipants") - GetIntProperty(tripElement, "availableSpots")
                            };
                            
                            // Process guides if present
                            if (tripElement.TryGetProperty("guides", out var guidesElement) && 
                                guidesElement.TryGetProperty("$values", out var guidesValues))
                            {
                                trip.Guides = new List<GuideModel>();
                                
                                foreach (var guideElement in guidesValues.EnumerateArray())
                                {
                                    var guide = new GuideModel
                                    {
                                        Id = GetIntProperty(guideElement, "id"),
                                        // Split name into first and last name
                                        FirstName = SplitName(GetStringProperty(guideElement, "name")).firstName,
                                        LastName = SplitName(GetStringProperty(guideElement, "name")).lastName,
                                        Bio = GetStringProperty(guideElement, "bio"),
                                        Email = GetStringProperty(guideElement, "email"),
                                        PhoneNumber = GetStringProperty(guideElement, "phone"),
                                        PhotoUrl = GetStringProperty(guideElement, "imageUrl"),
                                        YearsExperience = GetIntProperty(guideElement, "yearsOfExperience")
                                    };
                                    
                                    trip.Guides.Add(guide);
                                }
                            }
                            
                            trips.Add(trip);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error manually parsing trips JSON for destination {DestinationId}, falling back to automatic deserialization", destinationId);
                        
                        // Fallback to old method if parsing fails
                        try
                        {
                            trips = JsonSerializer.Deserialize<List<TripModel>>(content, jsonOptions) ?? new List<TripModel>();
                        }
                        catch
                        {
                            try
                            {
                                var jsonDoc = JsonDocument.Parse(content);
                                if (jsonDoc.RootElement.TryGetProperty("$values", out var valuesElement))
                                {
                                    trips = JsonSerializer.Deserialize<List<TripModel>>(valuesElement.GetRawText(), jsonOptions) ?? new List<TripModel>();
                                }
                            }
                            catch (Exception innerEx)
                            {
                                _logger.LogError(innerEx, "All parsing methods failed for destination trips JSON");
                                return new List<TripModel>();
                            }
                        }
                    }
                    
                    // Enrich trips with destination images if needed
                    await EnrichTripsWithDestinationImagesAsync(trips);
                    
                    return trips;
                }
                else
                {
                    // Log the error
                    _logger.LogWarning("API Error: {StatusCode} - {ErrorContent}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                }
                
                // Handle errors
                return new List<TripModel>();
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in GetTripsByDestinationAsync: {DestinationId}", destinationId);
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
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}Trip", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripModel>(responseContent, _jsonOptions);
                }
                
                // Handle errors
                _logger.LogWarning("Failed to create trip: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in CreateTripAsync");
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
                
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}Trip/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TripModel>(responseContent, _jsonOptions);
                }
                
                // Handle errors
                _logger.LogWarning("Failed to update trip {Id}: {StatusCode}", id, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in UpdateTripAsync: {Id}", id);
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
                
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}Trip/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to delete trip {Id}: {StatusCode}", id, response.StatusCode);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in DeleteTripAsync: {Id}", id);
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
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}Trip/{tripId}/guides/{guideId}", null);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to assign guide {GuideId} to trip {TripId}: {StatusCode}", 
                        guideId, tripId, response.StatusCode);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in AssignGuideToTripAsync: {TripId}, {GuideId}", tripId, guideId);
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
                
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}Trip/{tripId}/guides/{guideId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to remove guide {GuideId} from trip {TripId}: {StatusCode}", 
                        guideId, tripId, response.StatusCode);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in RemoveGuideFromTripAsync: {TripId}, {GuideId}", tripId, guideId);
                return false;
            }
        }

        /// <summary>
        /// Book a trip for the current user
        /// </summary>
        public async Task<bool> BookTripAsync(int tripId, int numberOfParticipants)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                // Get the trip to calculate total price
                var trip = await GetTripByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.LogWarning("Cannot book trip {TripId}: trip not found", tripId);
                    return false;
                }
                
                // Create the booking request
                var booking = new TripRegistrationModel
                {
                    TripId = tripId,
                    NumberOfParticipants = numberOfParticipants,
                    TotalPrice = trip.Price * numberOfParticipants,
                    Status = "Pending"
                };
                
                var json = JsonSerializer.Serialize(booking);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}TripRegistration", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to book trip {TripId}: {StatusCode}", tripId, response.StatusCode);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in BookTripAsync: {TripId}", tripId);
                return false;
            }
        }

        /// <summary>
        /// Get all trips booked by the current user
        /// </summary>
        public async Task<List<TripRegistrationModel>> GetUserTripsAsync()
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                // Get the current user ID
                var currentUser = await _httpClient.GetAsync($"{_apiBaseUrl}User/current");
                if (!currentUser.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get current user: {StatusCode}", currentUser.StatusCode);
                    return new List<TripRegistrationModel>();
                }
                
                var userContent = await currentUser.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserModel>(userContent, _jsonOptions);
                
                if (user == null)
                {
                    _logger.LogWarning("Current user is null");
                    return new List<TripRegistrationModel>();
                }
                
                // Get the bookings for this user
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}TripRegistration/user/{user.Id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var bookings = JsonSerializer.Deserialize<List<TripRegistrationModel>>(content, _jsonOptions);
                    return bookings ?? new List<TripRegistrationModel>();
                }
                
                // Handle errors
                _logger.LogWarning("Failed to get user trips: {StatusCode}", response.StatusCode);
                return new List<TripRegistrationModel>();
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in GetUserTripsAsync");
                return new List<TripRegistrationModel>();
            }
        }

        /// <summary>
        /// Enrich trips with destination images for those that don't have images
        /// </summary>
        private async Task EnrichTripsWithDestinationImagesAsync(List<TripModel> trips)
        {
            if (trips == null || !trips.Any())
                return;

            try
            {
                _logger.LogInformation("Starting image enrichment for {TripCount} trips", trips.Count);
                
                // Create a set of all destination IDs from the trips
                var destinationIds = trips.Select(t => t.DestinationId).Distinct().ToList();
                _logger.LogInformation("Found {DestCount} distinct destinations to load", destinationIds.Count);
                
                // Load all needed destinations at once for better performance
                var destinations = new Dictionary<int, DestinationModel>();
                
                foreach (var destId in destinationIds)
                {
                    var destination = await _destinationService.GetDestinationByIdAsync(destId);
                    if (destination != null)
                    {
                        destinations[destId] = destination;
                        _logger.LogInformation("Loaded destination {DestId}: {DestName} with ImageUrl: {ImageUrl}", 
                            destination.Id, destination.Name, destination.ImageUrl ?? "none");
                    }
                }
                
                // Enrich all trips with their destination data
                int enrichedTripCount = 0;
                foreach (var trip in trips)
                {
                    // Always set the destination name if available
                    if (destinations.TryGetValue(trip.DestinationId, out var destination))
                    {
                        trip.DestinationName = destination.Name;
                        
                        // If trip has no image but destination does, use the destination's image
                        if (string.IsNullOrEmpty(trip.ImageUrl) && !string.IsNullOrEmpty(destination.ImageUrl))
                        {
                            trip.ImageUrl = destination.ImageUrl;
                            enrichedTripCount++;
                            _logger.LogInformation("Enriched trip {TripId} with destination image from {DestId}", 
                                trip.Id, destination.Id);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Trip {TripId} references unknown destination {DestId}", 
                            trip.Id, trip.DestinationId);
                    }
                }
                
                _logger.LogInformation("Successfully enriched {EnrichedCount} trips with destination images", enrichedTripCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enriching trips with destination images");
            }
        }

        // Helper methods for JSON property extraction
        private string? GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
            {
                return prop.GetString();
            }
            return null;
        }
        
        private int GetIntProperty(JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
            {
                if (prop.TryGetInt32(out var value))
                {
                    return value;
                }
            }
            return defaultValue;
        }
        
        private decimal GetDecimalProperty(JsonElement element, string propertyName, decimal defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
            {
                if (prop.TryGetDecimal(out var value))
                {
                    return value;
                }
            }
            return defaultValue;
        }
        
        private DateTime GetDateTimeProperty(JsonElement element, string propertyName, DateTime? defaultValue = null)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind != JsonValueKind.Null)
            {
                if (prop.TryGetDateTime(out var value))
                {
                    return value;
                }
            }
            return defaultValue ?? DateTime.Now;
        }
        
        private (string firstName, string lastName) SplitName(string? fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return ("Unknown", "");
            }
            
            var parts = fullName.Split(' ', 2);
            return parts.Length > 1 
                ? (parts[0], parts[1]) 
                : (parts[0], "");
        }
    }
} 