using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Services
{
    /// <summary>
    /// Service for handling authentication operations that connects to the backend API
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        public AuthService(IHttpContextAccessor httpContextAccessor, ILogger<AuthService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7066/api/";
        }

        /// <summary>
        /// Authenticates a user with the provided credentials via the API
        /// </summary>
        public async Task<bool> LoginAsync(LoginViewModel loginModel)
        {
            try
            {
                // Create the login request object
                var loginRequest = new
                {
                    Username = loginModel.Username,
                    Password = loginModel.Password
                };

                // Convert to JSON
                var loginJson = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(loginJson, Encoding.UTF8, "application/json");

                // Make the API request
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseModel>();
                    
                    if (tokenResponse != null)
                    {
                        // Store the token in session
                        _httpContextAccessor.HttpContext?.Session.SetString("Token", tokenResponse.Token);
                        _httpContextAccessor.HttpContext?.Session.SetString("Username", tokenResponse.Username);
                        _httpContextAccessor.HttpContext?.Session.SetString("IsAdmin", tokenResponse.IsAdmin.ToString());
                        
                        // Create claims identity
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, tokenResponse.Username),
                            new Claim(ClaimTypes.NameIdentifier, tokenResponse.Username),
                            new Claim("Token", tokenResponse.Token)
                        };

                        if (tokenResponse.IsAdmin)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                        }

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = loginModel.RememberMe,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(loginModel.RememberMe ? 30 : 1)
                        };

                        // Store the token in the authentication ticket properties
                        authProperties.StoreTokens(new List<AuthenticationToken>
                        {
                            new AuthenticationToken
                            {
                                Name = "access_token",
                                Value = tokenResponse.Token
                            }
                        });

                        await _httpContextAccessor.HttpContext!.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);
                        
                        _logger.LogInformation("User {Username} logged in successfully", loginModel.Username);
                        return true;
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed login attempt for username: {Username}. Error: {Error}", 
                        loginModel.Username, errorContent);
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", loginModel.Username);
                return false;
            }
        }

        /// <summary>
        /// Registers a new user via the API
        /// </summary>
        public async Task<bool> RegisterAsync(RegisterViewModel registerModel)
        {
            try
            {
                // Create the registration request object
                var registerRequest = new
                {
                    Username = registerModel.Username,
                    Email = registerModel.Email,
                    Password = registerModel.Password,
                    ConfirmPassword = registerModel.ConfirmPassword,
                    FirstName = registerModel.FirstName,
                    LastName = registerModel.LastName,
                    PhoneNumber = registerModel.PhoneNumber,
                    Address = registerModel.Address
                };

                // Convert to JSON
                var registerJson = JsonSerializer.Serialize(registerRequest);
                var content = new StringContent(registerJson, Encoding.UTF8, "application/json");

                // Make the API request
                var response = await _httpClient.PostAsync($"{_apiBaseUrl}auth/register", content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("User {Username} registered successfully", registerModel.Username);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed registration attempt for username: {Username}. Error: {Error}", 
                        registerModel.Username, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for username: {Username}", registerModel.Username);
                return false;
            }
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                // Remove the token from session
                _httpContextAccessor.HttpContext?.Session.Remove("Token");
                _httpContextAccessor.HttpContext?.Session.Remove("Username");
                _httpContextAccessor.HttpContext?.Session.Remove("IsAdmin");
                
                // Sign out from cookie authentication
                await _httpContextAccessor.HttpContext!.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
                
                _logger.LogInformation("User logged out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        public async Task<UserModel?> GetCurrentUserAsync()
        {
            try
            {
                // Get token from session
                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("GetCurrentUserAsync failed: No token found in session");
                    return null;
                }
                
                // Clear any existing headers
                _httpClient.DefaultRequestHeaders.Authorization = null;
                
                // Set the authorization header exactly as in the curl example
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // Log the API request with full details for debugging
                _logger.LogInformation("Getting current user profile from {ApiUrl} with token: {TokenPreview}...", 
                    $"{_apiBaseUrl}User/current", 
                    token.Substring(0, Math.Min(20, token.Length)) + "...");
                
                // Make the API request to get the current user
                var response = await _httpClient.GetAsync($"{_apiBaseUrl}User/current");
                
                // Log full details of the response
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("API response: Status={StatusCode}, Content={Content}", 
                    response.StatusCode, 
                    responseContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var user = JsonSerializer.Deserialize<UserModel>(responseContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true 
                    });
                    
                    if (user != null)
                    {
                        _logger.LogInformation("Successfully retrieved user: {Username}", user.Username);
                        return user;
                    }
                    else
                    {
                        _logger.LogWarning("API returned success but user object was null");
                        return null;
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to get current user: API returned {StatusCode} with message: {ErrorMessage}",
                        response.StatusCode, responseContent);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
        }

        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        public bool IsAdmin()
        {
            return _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
        }
        
        /// <summary>
        /// Changes the password for the currently authenticated user
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
        {
            try
            {
                // Validate that the new password and confirm password match
                if (newPassword != confirmPassword)
                {
                    _logger.LogWarning("Password change failed: New password and confirmation do not match");
                    return false;
                }
                
                // Get the token from session
                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
                
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Password change failed: User not authenticated");
                    return false;
                }
                
                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                // Create the DTO for the API
                var changePasswordDto = new ChangePasswordDTO
                {
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmNewPassword = confirmPassword
                };
                
                // Log the API request for debugging
                _logger.LogInformation("Sending password change request to {ApiUrl}", $"{_apiBaseUrl}Auth/changepassword");
                
                // Make the API request to change password
                var response = await _httpClient.PostAsJsonAsync($"{_apiBaseUrl}Auth/changepassword", changePasswordDto);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Password changed successfully");
                    return true;
                }
                else
                {
                    // Read error message from response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Password change failed: API returned {StatusCode} with message: {ErrorMessage}", 
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change");
                return false;
            }
        }
    }
} 