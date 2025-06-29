using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Pages.Account
{
    /// <summary>
    /// Page for changing the user's password
    /// </summary>
    [Authorize] // Only authenticated users can access this page
    public class ChangePasswordModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(IAuthService authService, ILogger<ChangePasswordModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Input model for the change password form
        /// </summary>
        [BindProperty]
        public ChangePasswordViewModel Input { get; set; } = new ChangePasswordViewModel();

        /// <summary>
        /// Success message to display after password is changed
        /// </summary>
        [TempData]
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Error message to display if password change fails
        /// </summary>
        [TempData]
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// GET request handler
        /// </summary>
        public void OnGet()
        {
            // Page is displayed with the empty form
        }

        /// <summary>
        /// POST request handler for form submission
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Call the auth service to change the password
                var result = await _authService.ChangePasswordAsync(
                    Input.CurrentPassword,
                    Input.NewPassword,
                    Input.ConfirmPassword);

                if (result)
                {
                    _logger.LogInformation("User changed their password successfully.");
                    SuccessMessage = "Your password has been changed successfully.";
                    return RedirectToPage();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Password change failed. Please check your current password and try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                ErrorMessage = "An error occurred while changing your password. Please try again later.";
                return Page();
            }
        }
    }
} 