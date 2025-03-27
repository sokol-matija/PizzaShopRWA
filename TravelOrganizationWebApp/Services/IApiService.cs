using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TravelOrganizationWebApp.Services
{
    public interface IApiService
    {
        Task<HttpResponseMessage> GetAsync(string endpoint, string? token = null);
        Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, string? token = null) where T : class;
        Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, string? token = null) where T : class;
        Task<HttpResponseMessage> DeleteAsync(string endpoint, string? token = null);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl = "https://localhost:7066/api/"; // Make sure this matches your API URL

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint, string? token = null)
        {
            token ??= _httpContextAccessor.HttpContext?.Session.GetString("Token");
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _httpClient.GetAsync(_baseUrl + endpoint);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, string? token = null) where T : class
        {
            token ??= _httpContextAccessor.HttpContext?.Session.GetString("Token");
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            return await _httpClient.PostAsync(_baseUrl + endpoint, content);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, string? token = null) where T : class
        {
            token ??= _httpContextAccessor.HttpContext?.Session.GetString("Token");
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            return await _httpClient.PutAsync(_baseUrl + endpoint, content);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, string? token = null)
        {
            token ??= _httpContextAccessor.HttpContext?.Session.GetString("Token");
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _httpClient.DeleteAsync(_baseUrl + endpoint);
        }
    }
}