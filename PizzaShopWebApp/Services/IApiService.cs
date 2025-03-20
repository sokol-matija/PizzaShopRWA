using System.Net.Http.Headers;

namespace PizzaShopWebApp.Services
{
    public interface IApiService
    {
        Task<HttpClient> GetHttpClientAsync();
    }

    public abstract class ApiServiceBase : IApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ILogger _logger;
        protected readonly string _baseUrl;

        public ApiServiceBase(
            IHttpClientFactory httpClientFactory, 
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:16001";
        }

        public Task<HttpClient> GetHttpClientAsync()
        {
            var client = _httpClientFactory.CreateClient();
            
            // Set base URL
            client.BaseAddress = new Uri(_baseUrl);
            
            // Add authorization header if token exists
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            
            return Task.FromResult(client);
        }
    }
}