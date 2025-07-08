using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using WebApp.Models;

namespace WebApp.Services
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
            
            // First try to get token from session (like AuthService does)
            var sessionToken = httpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(sessionToken))
            {
                _logger.LogInformation("Using token from session for API request");
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
                    _logger.LogInformation("Using token from authentication cookie for API request");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cookieToken);
                    return;
                }
            }
            
            _logger.LogWarning("No authentication token found in session or cookie");
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
                            catch
                            {
                                _logger.LogError("Could not deserialize trips response from API");
                                throw;
                            }
                        }
                    }
                    
                    _logger.LogInformation("Successfully fetched {Count} trips from API", trips.Count);
                    
                    // Enrich trips with destination images
                    await EnrichTripsWithDestinationImagesAsync(trips);
                    
                    return trips;
                }
                else
                {
                    _logger.LogError("Failed to fetch trips from API. Status: {StatusCode}, Content: {Content}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    throw new HttpRequestException($"API request failed with status {response.StatusCode}");
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching trips from API");
                throw;
            }
        }

        /// <summary>
        /// Get trips with pagination support
        /// </summary>
        public async Task<(List<TripModel> trips, int totalCount)> GetTripsAsync(int page = 1, int pageSize = 10, int? destinationId = null)
        {
            try
            {
                _logger.LogInformation("Fetching trips with pagination - Page: {Page}, PageSize: {PageSize}, DestinationId: {DestinationId}", 
                    page, pageSize, destinationId);

                // For now, we'll get all trips and do pagination on the WebApp side
                // This can be optimized later to use API-side pagination
                List<TripModel> allTrips;
                
                if (destinationId.HasValue)
                {
                    allTrips = await GetTripsByDestinationAsync(destinationId.Value);
                }
                else
                {
                    allTrips = await GetAllTripsAsync();
                }

                // Apply pagination
                var totalCount = allTrips.Count;
                var paginatedTrips = allTrips
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                _logger.LogInformation("Returning {Count} trips out of {Total} for page {Page}", 
                    paginatedTrips.Count, totalCount, page);

                return (paginatedTrips, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching paginated trips");
                throw;
            }
        }

        /// <summary>
        /// Search trips with pagination support
        /// </summary>
        public async Task<(List<TripModel> trips, int totalCount)> SearchTripsAsync(string? name, string? description, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Searching trips - Name: '{Name}', Description: '{Description}', Page: {Page}, PageSize: {PageSize}", 
                    name, description, page, pageSize);

                // Use the new search API endpoint
                var queryParams = new List<string>();
                
                if (!string.IsNullOrWhiteSpace(name))
                    queryParams.Add($"name={Uri.EscapeDataString(name)}");
                
                if (!string.IsNullOrWhiteSpace(description))
                    queryParams.Add($"description={Uri.EscapeDataString(description)}");
                
                queryParams.Add($"page={page}");
                queryParams.Add($"count={pageSize}");

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Trip/search?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Response for search: {Content}", content);

                    var trips = await ParseTripsFromJsonAsync(content);
                    
                    // For now, we don't have total count from API, so we'll use the returned count
                    // This is a limitation that could be improved by modifying the API to return total count
                    var totalCount = trips.Count; // This is approximate - actual total might be higher

                    _logger.LogInformation("Search returned {Count} trips for page {Page}", trips.Count, page);

                    return (trips, totalCount);
                }
                else
                {
                    _logger.LogError("Failed to search trips from API. Status: {StatusCode}, Content: {Content}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    throw new HttpRequestException($"API search request failed with status {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching trips");
                throw;
            }
        }

        /// <summary>
        /// Helper method to parse trips from JSON response
        /// </summary>
        private async Task<List<TripModel>> ParseTripsFromJsonAsync(string jsonContent)
        {
            var trips = new List<TripModel>();
            
            try
            {
                var jsonDoc = JsonDocument.Parse(jsonContent);
                
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
                    _logger.LogWarning("Unexpected JSON structure in ParseTripsFromJsonAsync");
                    return trips;
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

                // Enrich trips with destination images
                await EnrichTripsWithDestinationImagesAsync(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing trips from JSON");
                throw;
            }

            return trips;
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
                
                // Map TripModel to CreateTripDTO with correct property names
                var createDto = new
                {
                    Name = trip.Title,  // API expects "Name" but WebApp uses "Title"
                    Description = trip.Description,
                    StartDate = trip.StartDate,
                    EndDate = trip.EndDate,
                    Price = trip.Price,
                    ImageUrl = trip.ImageUrl ?? string.Empty,
                    MaxParticipants = trip.Capacity,  // API expects "MaxParticipants" but WebApp uses "Capacity"
                    DestinationId = trip.DestinationId,
                    GuideIds = new List<int>()  // Empty list - guides will be assigned later via guide assignment page
                };
                
                var json = JsonSerializer.Serialize(createDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Creating trip with data: {Data}", json);
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}Trip", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Trip created successfully: {Title}", trip.Title);
                    
                    // Parse the response which is a TripDTO and map it back to TripModel
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var createdTrip = new TripModel
                    {
                        Id = GetIntProperty(jsonDoc.RootElement, "id"),
                        Title = GetStringProperty(jsonDoc.RootElement, "name") ?? string.Empty,
                        Description = GetStringProperty(jsonDoc.RootElement, "description") ?? string.Empty,
                        StartDate = GetDateTimeProperty(jsonDoc.RootElement, "startDate"),
                        EndDate = GetDateTimeProperty(jsonDoc.RootElement, "endDate"),
                        Price = GetDecimalProperty(jsonDoc.RootElement, "price"),
                        ImageUrl = GetStringProperty(jsonDoc.RootElement, "imageUrl"),
                        Capacity = GetIntProperty(jsonDoc.RootElement, "maxParticipants"),
                        DestinationId = GetIntProperty(jsonDoc.RootElement, "destinationId"),
                        DestinationName = GetStringProperty(jsonDoc.RootElement, "destinationName") ?? string.Empty
                    };
                    
                    return createdTrip;
                }
                
                // Handle errors - log the response content for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create trip: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
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
                
                // Map TripModel to UpdateTripDTO with correct property names
                var updateDto = new
                {
                    Id = trip.Id,
                    Name = trip.Title,  // API expects "Name" but WebApp uses "Title"
                    Description = trip.Description,
                    StartDate = trip.StartDate,
                    EndDate = trip.EndDate,
                    Price = trip.Price,
                    ImageUrl = trip.ImageUrl,
                    MaxParticipants = trip.Capacity,  // API expects "MaxParticipants" but WebApp uses "Capacity"
                    DestinationId = trip.DestinationId
                };
                
                var json = JsonSerializer.Serialize(updateDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Updating trip {Id} with data: {Data}", id, json);
                
                var response = await _httpClient.PutAsync($"{_apiBaseUrl}Trip/{id}", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Trip {Id} updated successfully", id);
                    
                    // Parse the response which is a TripDTO and map it back to TripModel
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    var updatedTrip = new TripModel
                    {
                        Id = GetIntProperty(jsonDoc.RootElement, "id"),
                        Title = GetStringProperty(jsonDoc.RootElement, "name") ?? string.Empty,
                        Description = GetStringProperty(jsonDoc.RootElement, "description") ?? string.Empty,
                        StartDate = GetDateTimeProperty(jsonDoc.RootElement, "startDate"),
                        EndDate = GetDateTimeProperty(jsonDoc.RootElement, "endDate"),
                        Price = GetDecimalProperty(jsonDoc.RootElement, "price"),
                        ImageUrl = GetStringProperty(jsonDoc.RootElement, "imageUrl"),
                        Capacity = GetIntProperty(jsonDoc.RootElement, "maxParticipants"),
                        DestinationId = GetIntProperty(jsonDoc.RootElement, "destinationId"),
                        DestinationName = GetStringProperty(jsonDoc.RootElement, "destinationName") ?? string.Empty
                    };
                    
                    return updatedTrip;
                }
                
                // Handle errors - log the response content for debugging
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update trip {Id}: {StatusCode}, Error: {Error}", id, response.StatusCode, errorContent);
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
                _logger.LogInformation("Attempting to remove guide {GuideId} from trip {TripId}", guideId, tripId);
                
                // Set authentication token
                await SetAuthHeaderAsync();
                
                // Log the URL being called and auth header status
                var deleteUrl = $"{_apiBaseUrl}Trip/{tripId}/guides/{guideId}";
                var hasAuthHeader = _httpClient.DefaultRequestHeaders.Authorization != null;
                _logger.LogInformation("Making DELETE request to: {Url}, Has Auth Header: {HasAuth}", deleteUrl, hasAuthHeader);
                
                var response = await _httpClient.DeleteAsync(deleteUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully removed guide {GuideId} from trip {TripId}", guideId, tripId);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to remove guide {GuideId} from trip {TripId}: {StatusCode} - {ErrorContent}", 
                        guideId, tripId, response.StatusCode, errorContent);
                    
                    // Additional debugging for 404 errors
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _logger.LogError("404 Not Found - URL: {Url}, TripId: {TripId}, GuideId: {GuideId}, BaseAddress: {BaseAddress}", 
                            deleteUrl, tripId, guideId, _httpClient.BaseAddress);
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in RemoveGuideFromTripAsync: TripId={TripId}, GuideId={GuideId}", tripId, guideId);
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
                
                // Get the trip to validate it exists
                var trip = await GetTripByIdAsync(tripId);
                if (trip == null)
                {
                    _logger.LogWarning("Cannot book trip {TripId}: trip not found", tripId);
                    return false;
                }
                
                // Get the current user ID
                var currentUser = await _httpClient.GetAsync($"{_apiBaseUrl}User/current");
                if (!currentUser.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get current user: {StatusCode}", currentUser.StatusCode);
                    return false;
                }
                
                var userContent = await currentUser.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserModel>(userContent, _jsonOptions);
                
                if (user == null)
                {
                    _logger.LogWarning("Current user is null");
                    return false;
                }
                
                // Create the booking request with only the essential fields
                var booking = new
                {
                    TripId = tripId,
                    UserId = user.Id,
                    NumberOfParticipants = numberOfParticipants
                };
                
                _logger.LogInformation("Creating booking for userId: {UserId}, tripId: {TripId}, participants: {Participants}", 
                    user.Id, tripId, numberOfParticipants);
                
                var json = JsonSerializer.Serialize(booking);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}TripRegistration", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to book trip {TripId}: {StatusCode}, Error: {Error}", 
                        tripId, response.StatusCode, errorContent);
                    return false;
                }
                
                _logger.LogInformation("Successfully booked trip {TripId} for user {UserId}", tripId, user.Id);
                return true;
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
                var currentUserContent = await currentUser.Content.ReadAsStringAsync();
                
                _logger.LogInformation("User/current API response: Status={Status}, Content={Content}", 
                    currentUser.StatusCode, currentUserContent);
                    
                if (!currentUser.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to get current user: {StatusCode}", currentUser.StatusCode);
                    return new List<TripRegistrationModel>();
                }
                
                var user = JsonSerializer.Deserialize<UserModel>(currentUserContent, _jsonOptions);
                
                if (user == null)
                {
                    _logger.LogWarning("Current user is null");
                    return new List<TripRegistrationModel>();
                }
                
                _logger.LogInformation("Successfully retrieved current user: ID={UserId}, Username={Username}", 
                    user.Id, user.Username);
                
                // Get the bookings for this user
                var registrationUrl = $"{_apiBaseUrl}TripRegistration/user/{user.Id}";
                _logger.LogInformation("Requesting user bookings from: {Url}", registrationUrl);
                
                var response = await _httpClient.GetAsync(registrationUrl);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("TripRegistration API response: Status={Status}, Content={Content}", 
                    response.StatusCode, responseContent);
                
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        // First, try parsing the response with standard options
                        var jsonOptions = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
                        };
                        
                        var bookings = JsonSerializer.Deserialize<List<TripRegistrationModel>>(responseContent, jsonOptions);
                        
                        if (bookings != null)
                        {
                            _logger.LogInformation("Successfully deserialized {Count} bookings", bookings.Count);
                            return bookings;
                        }
                        else
                        {
                            _logger.LogWarning("Deserialized bookings collection is null");
                        }
                        
                        // If direct parsing fails, try to extract from reference-preserving format
                        var responseObj = JsonSerializer.Deserialize<JsonDocument>(responseContent, jsonOptions);
                        if (responseObj?.RootElement.TryGetProperty("$values", out var valuesElement) == true)
                        {
                            _logger.LogInformation("Found $values property in response, trying to deserialize from it");
                            bookings = JsonSerializer.Deserialize<List<TripRegistrationModel>>(
                                valuesElement.GetRawText(), jsonOptions);
                                
                            if (bookings != null)
                            {
                                _logger.LogInformation("Successfully deserialized {Count} bookings from $values", bookings.Count);
                                return bookings;
                            }
                        }
                        
                        _logger.LogWarning("All deserialization attempts failed");
                        return new List<TripRegistrationModel>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deserializing bookings response");
                        return new List<TripRegistrationModel>();
                    }
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

        /// <summary>
        /// Cancel a trip booking for the current user
        /// </summary>
        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            try
            {
                // Set authentication token
                await SetAuthHeaderAsync();
                
                _logger.LogInformation("Attempting to cancel booking {BookingId}", bookingId);
                
                var response = await _httpClient.DeleteAsync($"{_apiBaseUrl}TripRegistration/{bookingId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to cancel booking {BookingId}: {StatusCode}, Error: {Error}", 
                        bookingId, response.StatusCode, errorContent);
                    return false;
                }
                
                _logger.LogInformation("Successfully cancelled booking {BookingId}", bookingId);
                return true;
            }
            catch (Exception ex)
            {
                // Log exception
                _logger.LogError(ex, "Exception in CancelBookingAsync: {BookingId}", bookingId);
                return false;
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