using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TravelOrganizationWebApp.Services
{
    /// <summary>
    /// Base class for all API services that provides common HTTP client functionality
    /// </summary>
    public abstract class ApiServiceBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        protected readonly ILogger _logger;
        private readonly string _baseApiUrl;

        protected ApiServiceBase(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
            
            // Get the API base URL from configuration or use a default
            _baseApiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7066/api/";
        }

        /// <summary>
        /// Gets an HTTP client with the authentication token if available
        /// </summary>
        protected async Task<HttpClient> GetHttpClientAsync()
        {
            var client = _httpClientFactory.CreateClient();
            
            // Set the base address for the client
            client.BaseAddress = new Uri(_baseApiUrl);
            
            // Get the token from the session if available
            var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
            
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
            return client;
        }
        
        /// <summary>
        /// Performs a GET request to the specified endpoint
        /// </summary>
        protected async Task<HttpResponseMessage> GetAsync(string endpoint)
        {
            try
            {
                var client = await GetHttpClientAsync();
                return await client.GetAsync(endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing GET request to {Endpoint}", endpoint);
                throw;
            }
        }
        
        /// <summary>
        /// Performs a POST request with the specified data
        /// </summary>
        protected async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data) where T : class
        {
            try
            {
                var client = await GetHttpClientAsync();
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                return await client.PostAsync(endpoint, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing POST request to {Endpoint}", endpoint);
                throw;
            }
        }
        
        /// <summary>
        /// Performs a PUT request with the specified data
        /// </summary>
        protected async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data) where T : class
        {
            try
            {
                var client = await GetHttpClientAsync();
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                return await client.PutAsync(endpoint, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing PUT request to {Endpoint}", endpoint);
                throw;
            }
        }
        
        /// <summary>
        /// Performs a DELETE request to the specified endpoint
        /// </summary>
        protected async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            try
            {
                var client = await GetHttpClientAsync();
                return await client.DeleteAsync(endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing DELETE request to {Endpoint}", endpoint);
                throw;
            }
        }
    }
} 