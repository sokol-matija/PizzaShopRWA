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
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IHttpClientFactory httpClientFactory, ILogger<RegisterModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Username is required")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address")]
            [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirm password is required")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
            public string? FirstName { get; set; }

            [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
            public string? LastName { get; set; }

            [Phone(ErrorMessage = "Please enter a valid phone number")]
            [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
            public string? PhoneNumber { get; set; }

            [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
            public string? Address { get; set; }
        }

        public void OnGet()
        {
            // Display any error messages
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Create HttpClient
                var client = _httpClientFactory.CreateClient();
                
                // Create the registration data from the input model
                var registerData = new
                {
                    Username = Input.Username,
                    Email = Input.Email,
                    Password = Input.Password,
                    ConfirmPassword = Input.ConfirmPassword,
                    FirstName = Input.FirstName ?? string.Empty,
                    LastName = Input.LastName ?? string.Empty,
                    PhoneNumber = Input.PhoneNumber ?? string.Empty,
                    Address = Input.Address ?? string.Empty
                };
                
                // Serialize to JSON
                var content = new StringContent(
                    JsonSerializer.Serialize(registerData),
                    Encoding.UTF8,
                    "application/json");
                
                // Make API request
                var response = await client.PostAsync("http://localhost:5000/api/Auth/register", content);
                
                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "Registration successful! You can now log in.";
                    return RedirectToPage("Login");
                }
                else
                {
                    // Read error message
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, "Registration failed: " + errorContent);
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration for user {Username}", Input.Username);
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again later.");
                return Page();
            }
        }
    }
} 