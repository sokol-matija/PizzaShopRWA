using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PizzaShopWebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;
        // API base URL
        private const string ApiBaseUrl = "https://localhost:7137";

        public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Username is required")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                try
                {
                    // Create HttpClient
                    var client = _httpClientFactory.CreateClient();
                    
                    // Prepare login data
                    var loginData = new
                    {
                        Username = Input.Username,
                        Password = Input.Password
                    };
                    
                    // Serialize to JSON
                    var content = new StringContent(
                        JsonSerializer.Serialize(loginData),
                        Encoding.UTF8,
                        "application/json");
                    
                    // Make API request to the correct endpoint
                    var response = await client.PostAsync($"{ApiBaseUrl}/api/Auth/login", content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response
                        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                        
                        if (tokenResponse != null)
                        {
                            // Store token in session or cookie
                            // This is a simplified example - in a real app, use a secure approach
                            HttpContext.Session.SetString("AuthToken", tokenResponse.Token);
                            HttpContext.Session.SetString("Username", tokenResponse.Username);
                            HttpContext.Session.SetString("ExpiresAt", tokenResponse.ExpiresAt);
                            HttpContext.Session.SetString("IsAdmin", tokenResponse.IsAdmin.ToString());
                            
                            _logger.LogInformation("User {Username} logged in successfully", Input.Username);
                            
                            return LocalRedirect(returnUrl);
                        }
                    }
                    else
                    {
                        // Read error message
                        var errorContent = await response.Content.ReadAsStringAsync();
                        ModelState.AddModelError(string.Empty, "Login failed: " + errorContent);
                        return Page();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during login for user {Username}", Input.Username);
                    ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again later.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        
        // Class to deserialize token response
        private class TokenResponse
        {
            public string Token { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public bool IsAdmin { get; set; }
            public string ExpiresAt { get; set; } = string.Empty;
        }
    }
} 