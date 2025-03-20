using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace PizzaShopWebApp.Pages
{
    public class TestLoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TestLoginModel> _logger;

        [BindProperty]
        public string Username { get; set; } = "user1";

        [BindProperty]
        public string Password { get; set; } = "Marvel247";

        public string? Result { get; set; }

        public TestLoginModel(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<TestLoginModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:16001";
                client.BaseAddress = new Uri(baseUrl);

                _logger.LogInformation("Testing login with: {Username}/{Password} at {BaseUrl}", 
                    Username, Password, baseUrl);

                var loginData = new { Username = Username, Password = Password };
                var response = await client.PostAsJsonAsync("/api/auth/login", loginData);
                
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    Result = $"SUCCESS!\nStatus: {response.StatusCode}\n\nResponse:\n{FormatJson(content)}";
                    _logger.LogInformation("Login test successful with {Username}", Username);
                }
                else
                {
                    Result = $"FAILED!\nStatus: {response.StatusCode}\n\nResponse:\n{content}";
                    _logger.LogWarning("Login test failed with {Username}", Username);
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                Result = $"ERROR: {ex.Message}";
                _logger.LogError(ex, "Error during login test");
                return Page();
            }
        }

        private string FormatJson(string json)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return json;
            }
        }
    }
} 