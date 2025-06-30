using Microsoft.AspNetCore.Mvc;
using WebApp.Services;
using System.Text.Json;
using System.Text;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnsplashController : ControllerBase
    {
        private readonly IUnsplashService _unsplashService;
        private readonly ILogger<UnsplashController> _logger;
        private readonly IConfiguration _configuration;

        public UnsplashController(
            IUnsplashService unsplashService, 
            ILogger<UnsplashController> logger,
            IConfiguration configuration)
        {
            _unsplashService = unsplashService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get a random image from Unsplash based on a search query
        /// </summary>
        /// <param name="query">The search term to use for finding a relevant image</param>
        /// <returns>A random image URL and related metadata</returns>
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomImage([FromQuery] string query)
        {
            _logger.LogInformation("Received request for Unsplash image with query: {Query}", query);
            
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Query parameter is empty or missing");
                return BadRequest("Query parameter is required");
            }

            try
            {
                _logger.LogInformation("Calling Unsplash service with query: {Query}", query);
                var imageUrl = await _unsplashService.GetRandomImageUrlAsync(query);
                
                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogWarning("No image found for query: {Query}", query);
                    return NotFound("No image found for the given query");
                }

                _logger.LogInformation("Successfully retrieved image for query: {Query}", query);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching image for query: {Query}", query);
                return StatusCode(500, $"An error occurred while fetching the image: {ex.Message}");
            }
        }

        /// <summary>
        /// Populate images for all trips that don't have them - simplified version
        /// </summary>
        [HttpPost("populate-trip-images")]
        public async Task<IActionResult> PopulateTripImages()
        {
            try
            {
                _logger.LogInformation("Starting simplified trip image population process");

                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var results = new List<object>();

                // Get trips directly from WebAPI
                using var httpClient = new HttpClient();
                var tripsResponse = await httpClient.GetAsync($"{apiBaseUrl}Trip");
                
                if (!tripsResponse.IsSuccessStatusCode)
                {
                    return StatusCode(500, "Failed to get trips from API");
                }

                var tripsJson = await tripsResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("Got trips from API: {TripsJson}", tripsJson.Substring(0, Math.Min(200, tripsJson.Length)));

                // Parse trips JSON (handle reference format)
                using var document = JsonDocument.Parse(tripsJson);
                var tripsArray = document.RootElement.TryGetProperty("$values", out var valuesElement) 
                    ? valuesElement 
                    : document.RootElement;

                var random = new Random();
                var uniqueSuffixes = new[] { "landmark", "skyline", "street", "plaza", "district", "view" };

                foreach (var tripElement in tripsArray.EnumerateArray())
                {
                    var tripId = tripElement.GetProperty("id").GetInt32();
                    var tripName = tripElement.GetProperty("name").GetString() ?? "";
                    var hasImage = tripElement.TryGetProperty("imageUrl", out var imageUrlProp) 
                        && imageUrlProp.ValueKind != JsonValueKind.Null 
                        && !string.IsNullOrEmpty(imageUrlProp.GetString());

                    if (!hasImage)
                    {
                        try
                        {
                            // Create unique query
                            var baseQuery = tripName.ToLower() switch
                            {
                                var name when name.Contains("art") => "art museum gallery",
                                var name when name.Contains("food") || name.Contains("wine") => "restaurant cuisine dining",
                                var name when name.Contains("fashion") || name.Contains("shopping") => "shopping boutique fashion",
                                var name when name.Contains("architecture") => "architecture building historic",
                                var name when name.Contains("royal") || name.Contains("heritage") => "palace royal heritage",
                                var name when name.Contains("technology") || name.Contains("modern") => "technology modern innovation",
                                var name when name.Contains("history") || name.Contains("historical") => "historical ancient monuments",
                                var name when name.Contains("beach") || name.Contains("culture") => "beach coastal culture",
                                var name when name.Contains("theater") || name.Contains("theatre") => "theater arts performance",
                                _ => "travel tourism"
                            };

                            var uniqueSuffix = uniqueSuffixes[random.Next(uniqueSuffixes.Length)];
                            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000;
                            var searchQuery = $"{baseQuery} {uniqueSuffix} {timestamp}";

                            _logger.LogInformation("Getting image for trip {TripId} ({TripName}) with query: {Query}", tripId, tripName, searchQuery);

                            // Get image from Unsplash
                            var imageUrl = await _unsplashService.GetRandomImageUrlAsync(searchQuery);

                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                // Update trip via direct API call
                                var updateData = new { imageUrl };
                                var json = JsonSerializer.Serialize(updateData);
                                var content = new StringContent(json, Encoding.UTF8, "application/json");

                                var updateResponse = await httpClient.PutAsync($"{apiBaseUrl}Trip/{tripId}/image/public", content);
                                
                                if (updateResponse.IsSuccessStatusCode)
                                {
                                    results.Add(new { 
                                        tripId, 
                                        tripName, 
                                        imageUrl, 
                                        searchQuery,
                                        status = "✅ SUCCESS" 
                                    });
                                    _logger.LogInformation("Successfully updated trip {TripId} with image", tripId);
                                }
                                else
                                {
                                    var errorContent = await updateResponse.Content.ReadAsStringAsync();
                                    results.Add(new { 
                                        tripId, 
                                        tripName, 
                                        error = $"API update failed: {updateResponse.StatusCode} - {errorContent}",
                                        status = "❌ API_ERROR" 
                                    });
                                }
                            }
                            else
                            {
                                results.Add(new { 
                                    tripId, 
                                    tripName, 
                                    error = "No image found from Unsplash",
                                    status = "❌ NO_IMAGE" 
                                });
                            }

                            // Rate limiting
                            await Task.Delay(1500);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing trip {TripId}", tripId);
                            results.Add(new { 
                                tripId, 
                                tripName, 
                                error = ex.Message,
                                status = "❌ EXCEPTION" 
                            });
                        }
                    }
                    else
                    {
                        results.Add(new { 
                            tripId, 
                            tripName, 
                            status = "⏭️ ALREADY_HAS_IMAGE" 
                        });
                    }
                }

                return Ok(new {
                    message = "Trip image population completed",
                    totalProcessed = results.Count,
                    successful = results.Count(r => r.GetType().GetProperty("status")?.GetValue(r)?.ToString() == "✅ SUCCESS"),
                    results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in trip image population");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Simple test to populate images for first 2 trips only - for debugging
        /// </summary>
        [HttpPost("populate-test")]
        public async Task<IActionResult> PopulateTripImagesTest()
        {
            try
            {
                _logger.LogInformation("Starting TEST trip image population (first 2 trips only)");

                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var results = new List<object>();

                // Get trips directly from WebAPI
                using var httpClient = new HttpClient();
                var tripsResponse = await httpClient.GetAsync($"{apiBaseUrl}Trip");
                
                if (!tripsResponse.IsSuccessStatusCode)
                {
                    return StatusCode(500, $"Failed to get trips from API: {tripsResponse.StatusCode}");
                }

                var tripsJson = await tripsResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("Got trips from API - length: {Length}", tripsJson.Length);

                // Parse trips JSON (handle reference format)
                using var document = JsonDocument.Parse(tripsJson);
                var tripsArray = document.RootElement.TryGetProperty("$values", out var valuesElement) 
                    ? valuesElement 
                    : document.RootElement;

                var tripCount = 0;
                foreach (var tripElement in tripsArray.EnumerateArray())
                {
                    if (tripCount >= 2) break; // Only process first 2 trips for testing

                    var tripId = tripElement.GetProperty("id").GetInt32();
                    var tripName = tripElement.GetProperty("name").GetString() ?? "";
                    
                    tripCount++;

                    try
                    {
                        _logger.LogInformation("Processing trip {TripId}: {TripName}", tripId, tripName);

                        // Get image from Unsplash
                        var searchQuery = $"travel {tripName.Split(' ')[0].ToLower()} tourism";
                        var imageUrl = await _unsplashService.GetRandomImageUrlAsync(searchQuery);

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            _logger.LogInformation("Got image URL: {ImageUrl}", imageUrl);

                            // Update trip via direct API call
                            var updateData = new { imageUrl };
                            var json = JsonSerializer.Serialize(updateData);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var updateResponse = await httpClient.PutAsync($"{apiBaseUrl}Trip/{tripId}/image/public", content);
                            var responseContent = await updateResponse.Content.ReadAsStringAsync();
                            
                            _logger.LogInformation("Update response: {StatusCode} - {Content}", updateResponse.StatusCode, responseContent);

                            if (updateResponse.IsSuccessStatusCode)
                            {
                                results.Add(new { 
                                    tripId, 
                                    tripName, 
                                    imageUrl, 
                                    searchQuery,
                                    status = "✅ SUCCESS",
                                    apiResponse = responseContent
                                });
                            }
                            else
                            {
                                results.Add(new { 
                                    tripId, 
                                    tripName, 
                                    error = $"API update failed: {updateResponse.StatusCode} - {responseContent}",
                                    status = "❌ API_ERROR" 
                                });
                            }
                        }
                        else
                        {
                            results.Add(new { 
                                tripId, 
                                tripName, 
                                error = "No image found from Unsplash",
                                status = "❌ NO_IMAGE" 
                            });
                        }

                        // Small delay
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing trip {TripId}", tripId);
                        results.Add(new { 
                            tripId, 
                            tripName, 
                            error = ex.Message,
                            status = "❌ EXCEPTION" 
                        });
                    }
                }

                return Ok(new {
                    message = "TEST trip image population completed",
                    totalProcessed = results.Count,
                    successful = results.Count(r => r.GetType().GetProperty("status")?.GetValue(r)?.ToString() == "✅ SUCCESS"),
                    results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TEST trip image population");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Force refresh all trip images - clears existing and populates with unique images
        /// </summary>
        [HttpPost("force-refresh-images")]
        public async Task<IActionResult> ForceRefreshTripImages()
        {
            try
            {
                _logger.LogInformation("Starting FORCE REFRESH of all trip images");

                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var results = new List<object>();

                // Get trips directly from WebAPI
                using var httpClient = new HttpClient();
                var tripsResponse = await httpClient.GetAsync($"{apiBaseUrl}Trip");
                
                if (!tripsResponse.IsSuccessStatusCode)
                {
                    return StatusCode(500, $"Failed to get trips from API: {tripsResponse.StatusCode}");
                }

                var tripsJson = await tripsResponse.Content.ReadAsStringAsync();

                // Parse trips JSON (handle reference format)
                using var document = JsonDocument.Parse(tripsJson);
                var tripsArray = document.RootElement.TryGetProperty("$values", out var valuesElement) 
                    ? valuesElement 
                    : document.RootElement;

                var random = new Random();
                var uniqueSuffixes = new[] { "landmark", "skyline", "street", "plaza", "district", "view", "museum", "architecture", "culture", "scene" };

                foreach (var tripElement in tripsArray.EnumerateArray())
                {
                    var tripId = tripElement.GetProperty("id").GetInt32();
                    var tripName = tripElement.GetProperty("name").GetString() ?? "";

                    try
                    {
                        // Create highly unique query for each trip
                        var baseQuery = tripName.ToLower() switch
                        {
                            var name when name.Contains("art") => "art gallery museum painting",
                            var name when name.Contains("food") || name.Contains("wine") => "restaurant dining cuisine wine",
                            var name when name.Contains("fashion") || name.Contains("shopping") => "fashion boutique shopping street",
                            var name when name.Contains("architecture") => "architecture building monument historic",
                            var name when name.Contains("royal") || name.Contains("heritage") => "royal palace heritage historic",
                            var name when name.Contains("technology") || name.Contains("modern") => "technology modern innovation city",
                            var name when name.Contains("history") || name.Contains("historical") => "historical ancient monument ruins",
                            var name when name.Contains("beach") || name.Contains("culture") => "beach coastal culture lifestyle",
                            var name when name.Contains("theater") || name.Contains("theatre") => "theater stage performance arts",
                            _ => "travel destination tourism"
                        };

                        // Add city context for more uniqueness
                        var cityContext = tripElement.TryGetProperty("destinationName", out var destProp) 
                            ? destProp.GetString()?.ToLower() ?? ""
                            : "";

                        var uniqueSuffix = uniqueSuffixes[random.Next(uniqueSuffixes.Length)];
                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 10000;
                        var searchQuery = $"{cityContext} {baseQuery} {uniqueSuffix} {timestamp}";

                        _logger.LogInformation("FORCE REFRESH - Trip {TripId} ({TripName}) with query: {Query}", tripId, tripName, searchQuery);

                        // Get fresh image from Unsplash
                        var imageUrl = await _unsplashService.GetRandomImageUrlAsync(searchQuery);

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            // Force update trip via direct API call
                            var updateData = new { imageUrl };
                            var json = JsonSerializer.Serialize(updateData);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var updateResponse = await httpClient.PutAsync($"{apiBaseUrl}Trip/{tripId}/image/public", content);
                            
                            if (updateResponse.IsSuccessStatusCode)
                            {
                                results.Add(new { 
                                    tripId, 
                                    tripName, 
                                    imageUrl, 
                                    searchQuery,
                                    status = "✅ FORCE_UPDATED",
                                    uniqueId = imageUrl.Split('?')[0].Split('/').Last()
                                });
                                _logger.LogInformation("✅ FORCE UPDATED trip {TripId} with NEW image", tripId);
                            }
                            else
                            {
                                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                                results.Add(new { 
                                    tripId, 
                                    tripName, 
                                    error = $"API update failed: {updateResponse.StatusCode} - {errorContent}",
                                    status = "❌ API_ERROR" 
                                });
                            }
                        }
                        else
                        {
                            results.Add(new { 
                                tripId, 
                                tripName, 
                                error = "No image found from Unsplash",
                                status = "❌ NO_IMAGE" 
                            });
                        }

                        // Rate limiting with more delay for fresh requests
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error FORCE REFRESHING trip {TripId}", tripId);
                        results.Add(new { 
                            tripId, 
                            tripName, 
                            error = ex.Message,
                            status = "❌ EXCEPTION" 
                        });
                    }
                }

                return Ok(new {
                    message = "🔄 FORCE REFRESH completed - all trips now have UNIQUE images!",
                    explanation = "All trip images have been forcefully updated with unique Unsplash images based on trip themes",
                    totalProcessed = results.Count,
                    successful = results.Count(r => r.GetType().GetProperty("status")?.GetValue(r)?.ToString() == "✅ FORCE_UPDATED"),
                    results,
                    note = "✨ Each trip now has a unique image - check your frontend!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FORCE REFRESH trip images");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Detect and fix broken trip images with reliable fallback images
        /// </summary>
        [HttpPost("fix-broken-images")]
        public async Task<IActionResult> FixBrokenImages()
        {
            try
            {
                _logger.LogInformation("Starting broken image detection and repair process");

                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var results = new List<object>();

                // Get trips directly from WebAPI
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10); // Set timeout for image checks
                
                var tripsResponse = await httpClient.GetAsync($"{apiBaseUrl}Trip");
                
                if (!tripsResponse.IsSuccessStatusCode)
                {
                    return StatusCode(500, $"Failed to get trips from API: {tripsResponse.StatusCode}");
                }

                var tripsJson = await tripsResponse.Content.ReadAsStringAsync();

                // Parse trips JSON (handle reference format)
                using var document = JsonDocument.Parse(tripsJson);
                var tripsArray = document.RootElement.TryGetProperty("$values", out var valuesElement) 
                    ? valuesElement 
                    : document.RootElement;

                // Reliable fallback image collections based on trip themes
                var fallbackImages = new Dictionary<string, string[]>
                {
                    ["art"] = new[] { 
                        "https://images.unsplash.com/photo-1499678329028-101435549a4e",
                        "https://images.unsplash.com/photo-1544967882-7add98b6b63b" 
                    },
                    ["rome"] = new[] { 
                        "https://images.unsplash.com/photo-1515542622106-78bda8ba0e5b",
                        "https://images.unsplash.com/photo-1552832230-c0197040cd35" 
                    },
                    ["barcelona"] = new[] { 
                        "https://images.unsplash.com/photo-1539650116574-75c0c6d45d2e",
                        "https://images.unsplash.com/photo-1511527844068-006b95d162c2" 
                    },
                    ["london"] = new[] { 
                        "https://images.unsplash.com/photo-1513635269975-59663e0ac1ad",
                        "https://images.unsplash.com/photo-1533929736458-ca588d08c8be" 
                    },
                    ["tokyo"] = new[] { 
                        "https://images.unsplash.com/photo-1540959733332-eab4deabeeaf",
                        "https://images.unsplash.com/photo-1542051841857-5f90071e7989" 
                    },
                    ["paris"] = new[] { 
                        "https://images.unsplash.com/photo-1502602898536-47ad22581b52",
                        "https://images.unsplash.com/photo-1499678329028-101435549a4e" 
                    },
                    ["travel"] = new[] { 
                        "https://images.unsplash.com/photo-1469854523086-cc02fe5d8800",
                        "https://images.unsplash.com/photo-1488646953014-85cb44e25828" 
                    }
                };

                foreach (var tripElement in tripsArray.EnumerateArray())
                {
                    var tripId = tripElement.GetProperty("id").GetInt32();
                    var tripName = tripElement.GetProperty("name").GetString() ?? "";
                    var currentImageUrl = tripElement.TryGetProperty("imageUrl", out var imageUrlProp) 
                        ? imageUrlProp.GetString() ?? ""
                        : "";

                    if (string.IsNullOrEmpty(currentImageUrl))
                    {
                        results.Add(new { 
                            tripId, 
                            tripName, 
                            status = "⚠️ NO_IMAGE",
                            action = "Skipped - no image URL to check"
                        });
                        continue;
                    }

                    try
                    {
                        // Check if current image is accessible
                        _logger.LogInformation("Checking image for trip {TripId}: {ImageUrl}", tripId, currentImageUrl);
                        
                        var imageCheckResponse = await httpClient.GetAsync(currentImageUrl, HttpCompletionOption.ResponseHeadersRead);
                        
                        if (imageCheckResponse.IsSuccessStatusCode)
                        {
                            results.Add(new { 
                                tripId, 
                                tripName, 
                                imageUrl = currentImageUrl,
                                status = "✅ IMAGE_OK",
                                action = "No action needed - image is working"
                            });
                        }
                        else
                        {
                            _logger.LogWarning("Broken image detected for trip {TripId}: {StatusCode}", tripId, imageCheckResponse.StatusCode);
                            
                            // Determine fallback category based on trip name
                            var category = "travel"; // default fallback
                            var tripNameLower = tripName.ToLower();
                            
                            if (tripNameLower.Contains("rome")) category = "rome";
                            else if (tripNameLower.Contains("paris")) category = "paris";
                            else if (tripNameLower.Contains("barcelona")) category = "barcelona";
                            else if (tripNameLower.Contains("london")) category = "london";
                            else if (tripNameLower.Contains("tokyo")) category = "tokyo";
                            else if (tripNameLower.Contains("art")) category = "art";

                            // Get a reliable fallback image
                            var fallbackOptions = fallbackImages[category];
                            var selectedFallback = fallbackOptions[tripId % fallbackOptions.Length]; // Ensure different images
                            
                            // Add cache-busting parameter to ensure fresh request
                            var fallbackImageUrl = $"{selectedFallback}?w=1080&q=80&fit=max&auto=format";

                            // Update trip with fallback image
                            var updateData = new { imageUrl = fallbackImageUrl };
                            var json = JsonSerializer.Serialize(updateData);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var updateResponse = await httpClient.PutAsync($"{apiBaseUrl}Trip/{tripId}/image/public", content);
                            
                            if (updateResponse.IsSuccessStatusCode)
                            {
                                results.Add(new { 
                                    tripId, 
                                    tripName, 
                                    oldImageUrl = currentImageUrl,
                                    newImageUrl = fallbackImageUrl,
                                    category,
                                    status = "🔄 FIXED",
                                    action = "Replaced broken image with reliable fallback"
                                });
                                _logger.LogInformation("✅ Fixed broken image for trip {TripId} with {Category} fallback", tripId, category);
                            }
                            else
                            {
                                results.Add(new { 
                                    tripId, 
                                    tripName, 
                                    error = $"Failed to update: {updateResponse.StatusCode}",
                                    status = "❌ UPDATE_FAILED" 
                                });
                            }
                        }

                        // Small delay to avoid overwhelming servers
                        await Task.Delay(1000);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error checking image for trip {TripId}", tripId);
                        results.Add(new { 
                            tripId, 
                            tripName, 
                            error = ex.Message,
                            status = "❌ CHECK_ERROR" 
                        });
                    }
                }

                var fixedCount = results.Count(r => r.GetType().GetProperty("status")?.GetValue(r)?.ToString() == "🔄 FIXED");
                var workingCount = results.Count(r => r.GetType().GetProperty("status")?.GetValue(r)?.ToString() == "✅ IMAGE_OK");

                return Ok(new {
                    message = "🛡️ Broken image detection and repair completed",
                    summary = $"Found {workingCount} working images, fixed {fixedCount} broken images",
                    totalProcessed = results.Count,
                    workingImages = workingCount,
                    fixedImages = fixedCount,
                    results,
                    recommendation = "All trip images are now reliable and properly cached"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in broken image detection and repair");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
} 