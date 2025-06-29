using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    /// <summary>
    /// View model for the login form
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username or Email")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }

        // Return URL for redirect after login
        public string? ReturnUrl { get; set; }
    }
} 