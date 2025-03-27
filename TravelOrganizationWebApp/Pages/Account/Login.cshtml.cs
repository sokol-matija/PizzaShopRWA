using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaShopWebApp.Services;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace PizzaShopWebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IUserService userService, ILogger<LoginModel> logger)
        {
            _userService = userService;
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
                    // Log user input for debugging
                    _logger.LogInformation("Login attempt - Username: {Username}, Password length: {PasswordLength}", 
                        Input.Username, Input.Password?.Length ?? 0);
                        
                    // Special case for the user1/Marvel247 test credentials
                    if (Input.Username == "user1" && Input.Password == "Marvel247")
                    {
                        _logger.LogInformation("Detected test credentials - special logging enabled");
                    }
                    
                    // Make sure password is not null before calling LoginAsync
                    if (Input.Password == null)
                    {
                        ModelState.AddModelError(string.Empty, "Password cannot be empty.");
                        return Page();
                    }

                    var result = await _userService.LoginAsync(Input.Username, Input.Password);
                    
                    if (result)
                    {
                        _logger.LogInformation("User {Username} logged in successfully", Input.Username);
                        
                        // Redirect to Dashboard after successful login
                        return RedirectToPage("/Dashboard/Index");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your username and password.");
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
    }
} 