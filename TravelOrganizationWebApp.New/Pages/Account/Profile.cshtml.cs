using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using System;

namespace TravelOrganizationWebApp.Pages.Account
{
    /// <summary>
    /// Page for displaying and editing user profile information
    /// </summary>
    [Authorize] // Only authenticated users can access this page
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(IAuthService authService, ILogger<ProfileModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// The current user's information
        /// </summary>
        public UserModel? CurrentUser { get; set; }

        /// <summary>
        /// Error message to display if profile loading fails
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Detailed error for debugging (only shown in development)
        /// </summary>
        public string? DetailedError { get; set; }

        /// <summary>
        /// GET request handler to load user profile
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Log authentication status for debugging
                _logger.LogInformation("User is authenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);
                
                // Log session token for debugging
                var token = HttpContext.Session.GetString("Token");
                _logger.LogInformation("Token exists in session: {HasToken}, Token prefix: {TokenPrefix}", 
                    !string.IsNullOrEmpty(token),
                    !string.IsNullOrEmpty(token) ? token.Substring(0, Math.Min(10, token.Length)) + "..." : "N/A");

                // Try to get token from authentication properties if not in session
                if (string.IsNullOrEmpty(token))
                {
                    var authResult = await HttpContext.AuthenticateAsync();
                    if (authResult.Succeeded)
                    {
                        token = authResult.Properties?.GetTokenValue("access_token");
                        _logger.LogInformation("Token exists in auth properties: {HasToken}, Token prefix: {TokenPrefix}", 
                            !string.IsNullOrEmpty(token),
                            !string.IsNullOrEmpty(token) ? token.Substring(0, Math.Min(10, token.Length)) + "..." : "N/A");
                    }
                }
                
                // Get the current user's profile information
                CurrentUser = await _authService.GetCurrentUserAsync();
                
                if (CurrentUser == null)
                {
                    _logger.LogWarning("Failed to load user profile: User not found");
                    ErrorMessage = "Failed to load your profile information. Please try again later.";
                    DetailedError = "API call returned null. Check logs for more details.";
                }
                else
                {
                    _logger.LogInformation("Successfully loaded profile for user: {Username}", CurrentUser.Username);
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                ErrorMessage = "An error occurred while loading your profile. Please try again later.";
                DetailedError = $"Exception: {ex.Message}";
                return Page();
            }
        }
    }
} 