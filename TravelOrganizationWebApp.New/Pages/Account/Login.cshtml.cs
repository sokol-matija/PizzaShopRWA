using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelOrganizationWebApp.Services;
using TravelOrganizationWebApp.ViewModels;

namespace TravelOrganizationWebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public LoginViewModel LoginInput { get; set; } = new LoginViewModel();

        public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            // If user is already logged in, redirect to home
            if (_authService.IsAuthenticated())
            {
                return RedirectToPage("/Index");
            }

            LoginInput.ReturnUrl = returnUrl ?? Url.Content("~/");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _authService.LoginAsync(LoginInput);

            if (result)
            {
                _logger.LogInformation("User {Username} logged in successfully", LoginInput.Username);
                
                // Check if the return URL is local to avoid open redirect attacks
                if (!string.IsNullOrEmpty(LoginInput.ReturnUrl) && Url.IsLocalUrl(LoginInput.ReturnUrl))
                {
                    return LocalRedirect(LoginInput.ReturnUrl);
                }
                else
                {
                    return RedirectToPage("/Index");
                }
            }

            // Login failed
            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your username and password.");
            _logger.LogWarning("Failed login attempt for user {Username}", LoginInput.Username);
            return Page();
        }
    }
} 