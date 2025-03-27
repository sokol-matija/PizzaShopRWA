using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelOrganizationWebApp.Services;
using TravelOrganizationWebApp.ViewModels;

namespace TravelOrganizationWebApp.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public RegisterViewModel RegisterInput { get; set; } = new RegisterViewModel();
        
        [TempData]
        public string? SuccessMessage { get; set; }

        public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // If user is already logged in, redirect to home
            if (_authService.IsAuthenticated())
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.RegisterAsync(RegisterInput);

            if (result)
            {
                _logger.LogInformation("User {Username} registered successfully", RegisterInput.Username);
                
                // Set temporary success message for the login page
                SuccessMessage = "Registration successful! You can now log in with your credentials.";
                
                return RedirectToPage("./Login");
            }

            // Registration failed
            ModelState.AddModelError(string.Empty, "Registration failed. Username or email may already be in use.");
            _logger.LogWarning("Failed registration attempt for user {Username}", RegisterInput.Username);
            return Page();
        }
    }
} 