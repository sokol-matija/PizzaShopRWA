using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IAuthService authService, ILogger<LogoutModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (_authService.IsAuthenticated())
            {
                await _authService.LogoutAsync();
                _logger.LogInformation("User logged out");
            }

            // Show the logout page with the spinner
            return Page();
        }
    }
} 