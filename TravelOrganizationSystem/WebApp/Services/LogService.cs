using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.Text.Json;
using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Service for managing system logs by communicating with the WebAPI
    /// Handles authentication and data conversion for admin log viewing
    /// </summary>
    public class LogService : ILogService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _apiBaseUrl;
        private readonly ILogger<LogService> _logger;

        public LogService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<LogService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;

            // Set up JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Set the API base URL
            _apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7199/api/";
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
            
            // First try to get token from session
            var sessionToken = httpContext.Session.GetString("Token");
            if (!string.IsNullOrEmpty(sessionToken))
            {
                _logger.LogInformation("Using token from session for logs API request");
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
                    _logger.LogInformation("Using token from authentication cookie for logs API request");
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cookieToken);
                    return;
                }
            }
            
            _logger.LogWarning("No authentication token found for logs API request");
        }

        /// <summary>
        /// Get the most recent log entries up to the specified count
        /// </summary>
        public async Task<List<LogModel>> GetLogsAsync(int count)
        {
            try
            {
                _logger.LogInformation("Fetching {Count} logs from API", count);
                
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Logs/get/{count}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Response for logs: {Content}", content);
                    
                    var logs = await ParseLogsFromJsonAsync(content);
                    
                    _logger.LogInformation("Successfully fetched {Count} logs from API", logs.Count);
                    return logs;
                }
                else
                {
                    _logger.LogError("Failed to fetch logs from API. Status: {StatusCode}, Content: {Content}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    throw new HttpRequestException($"API request failed with status {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching logs from API");
                throw;
            }
        }

        /// <summary>
        /// Get the total count of log entries in the system
        /// </summary>
        public async Task<int> GetLogsCountAsync()
        {
            try
            {
                _logger.LogInformation("Fetching logs count from API");
                
                // Set authentication token
                await SetAuthHeaderAsync();
                
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}Logs/count");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogDebug("API Response for logs count: {Content}", content);
                    
                    var countResponse = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
                    var count = countResponse.GetProperty("count").GetInt32();
                    
                    _logger.LogInformation("Successfully fetched logs count from API: {Count}", count);
                    return count;
                }
                else
                {
                    _logger.LogError("Failed to fetch logs count from API. Status: {StatusCode}, Content: {Content}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    throw new HttpRequestException($"API request failed with status {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching logs count from API");
                throw;
            }
        }

        /// <summary>
        /// Get logs with pagination support
        /// </summary>
        public async Task<(List<LogModel> logs, int totalCount)> GetLogsAsync(int page = 1, int pageSize = 50)
        {
            try
            {
                _logger.LogInformation("Fetching logs with pagination - Page: {Page}, PageSize: {PageSize}", page, pageSize);

                // Get total count first
                var totalCount = await GetLogsCountAsync();
                
                // Calculate how many logs to get for this page
                var skip = (page - 1) * pageSize;
                var logsToGet = Math.Min(pageSize, totalCount - skip);
                
                if (logsToGet <= 0)
                {
                    return (new List<LogModel>(), totalCount);
                }
                
                // For pagination, we need to get more logs than needed and then skip to the right page
                // Since the API only supports getting the most recent N logs, we need to get enough logs
                // to cover all previous pages plus the current page
                var totalLogsNeeded = skip + pageSize;
                var allLogs = await GetLogsAsync(Math.Min(totalLogsNeeded, totalCount));
                
                // Apply pagination to the results
                var paginatedLogs = allLogs.Skip(skip).Take(pageSize).ToList();
                
                _logger.LogInformation("Returning {Count} logs out of {Total} for page {Page}", 
                    paginatedLogs.Count, totalCount, page);

                return (paginatedLogs, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching paginated logs");
                throw;
            }
        }

        /// <summary>
        /// Helper method to parse logs from JSON response
        /// </summary>
        private async Task<List<LogModel>> ParseLogsFromJsonAsync(string jsonContent)
        {
            var logs = new List<LogModel>();
            
            try
            {
                var jsonDoc = JsonDocument.Parse(jsonContent);
                
                // Handle different response formats ($values array or direct array)
                JsonElement logsArray;
                if (jsonDoc.RootElement.TryGetProperty("$values", out var valuesElement))
                {
                    logsArray = valuesElement;
                }
                else if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    logsArray = jsonDoc.RootElement;
                }
                else
                {
                    _logger.LogWarning("Unexpected JSON structure in ParseLogsFromJsonAsync");
                    return logs;
                }
                
                // Process each log in the array
                foreach (var logElement in logsArray.EnumerateArray())
                {
                    var log = new LogModel
                    {
                        Id = GetIntProperty(logElement, "id"),
                        Timestamp = GetDateTimeProperty(logElement, "timestamp"),
                        Level = GetStringProperty(logElement, "level") ?? "Info",
                        Message = GetStringProperty(logElement, "message") ?? string.Empty
                    };
                    
                    logs.Add(log);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing logs JSON");
                
                // Fallback to direct deserialization
                try
                {
                    logs = JsonSerializer.Deserialize<List<LogModel>>(jsonContent, _jsonOptions) ?? new List<LogModel>();
                }
                catch
                {
                    _logger.LogError("Could not deserialize logs response from API");
                    throw;
                }
            }
            
            return logs;
        }

        /// <summary>
        /// Helper method to get string property from JSON element
        /// </summary>
        private string? GetStringProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) && property.ValueKind != JsonValueKind.Null
                ? property.GetString()
                : null;
        }

        /// <summary>
        /// Helper method to get int property from JSON element
        /// </summary>
        private int GetIntProperty(JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.TryGetInt32(out var value))
                    return value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Helper method to get DateTime property from JSON element
        /// </summary>
        private DateTime GetDateTimeProperty(JsonElement element, string propertyName, DateTime? defaultValue = null)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.TryGetDateTime(out var value))
                    return value;
            }
            return defaultValue ?? DateTime.MinValue;
        }
    }
} 