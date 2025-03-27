using System.Net.Http.Json;
using PizzaShopWebApp.Models;

namespace PizzaShopWebApp.Services
{
    public class UserService : ApiServiceBase, IUserService
    {
        public UserService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<UserService> logger) 
            : base(httpClientFactory, httpContextAccessor, configuration, logger)
        {
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var loginData = new { Username = username, Password = password };
                
                _logger.LogInformation("Attempting login with username: {Username}", username);
                
                var response = await client.PostAsJsonAsync("/api/auth/login", loginData);
                
                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseModel>();
                    
                    if (tokenResponse != null)
                    {
                        // Store token in session
                        _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", tokenResponse.Token);
                        _httpContextAccessor.HttpContext?.Session.SetString("Username", tokenResponse.Username);
                        _httpContextAccessor.HttpContext?.Session.SetString("IsAdmin", tokenResponse.IsAdmin.ToString());
                        _httpContextAccessor.HttpContext?.Session.SetString("ExpiresAt", tokenResponse.ExpiresAt);
                        
                        return true;
                    }
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed. Status code: {StatusCode}, Error: {Error}, Username: {Username}, Password: {Password}", 
                    response.StatusCode, errorContent, username, password);
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", username);
                return false;
            }
        }

        public async Task<bool> RegisterAsync(RegisterModel model)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.PostAsJsonAsync("/api/auth/register", model);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var changePasswordData = new 
                { 
                    CurrentPassword = currentPassword, 
                    NewPassword = newPassword,
                    ConfirmNewPassword = newPassword 
                };
                
                var response = await client.PostAsJsonAsync("/api/auth/changepassword", changePasswordData);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return false;
            }
        }

        public UserProfileModel GetUserProfileAsync()
        {
            // This would be a call to an endpoint that returns user profile info
            // Since it doesn't seem to exist in your current API, we'd need to add it
            // For now, let's return a basic profile from session data
            
            var username = _httpContextAccessor.HttpContext?.Session.GetString("Username");
            var isAdmin = _httpContextAccessor.HttpContext?.Session.GetString("IsAdmin") == "True";
            
            if (string.IsNullOrEmpty(username))
                return new UserProfileModel();
                
            return new UserProfileModel
            {
                Username = username,
                IsAdmin = isAdmin
                // Other fields would be populated from the API
            };
        }

        public bool UpdateUserProfileAsync(UserProfileModel profile)
        {
            // This would update the user profile via an API endpoint
            // Since it doesn't exist yet, implementation is pending
            _logger.LogWarning("UpdateUserProfileAsync not implemented yet");
            return false;
        }

        public Task LogoutAsync()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
            return Task.CompletedTask;
        }

        public bool IsAuthenticated()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            return !string.IsNullOrEmpty(token);
        }

        public bool IsAdmin()
        {
            var isAdmin = _httpContextAccessor.HttpContext?.Session.GetString("IsAdmin");
            return isAdmin == "True";
        }
    }
} 