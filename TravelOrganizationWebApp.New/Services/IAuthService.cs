using TravelOrganizationWebApp.Models;
using TravelOrganizationWebApp.ViewModels;

namespace TravelOrganizationWebApp.Services
{
    /// <summary>
    /// Interface for authentication services
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with the provided credentials
        /// </summary>
        /// <param name="loginModel">The login credentials</param>
        /// <returns>True if login successful, false otherwise</returns>
        Task<bool> LoginAsync(LoginViewModel loginModel);
        
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="registerModel">The registration data</param>
        /// <returns>True if registration successful, false otherwise</returns>
        Task<bool> RegisterAsync(RegisterViewModel registerModel);
        
        /// <summary>
        /// Logs out the current user
        /// </summary>
        Task LogoutAsync();
        
        /// <summary>
        /// Gets the current authenticated user
        /// </summary>
        /// <returns>The user model if authenticated, null otherwise</returns>
        Task<UserModel?> GetCurrentUserAsync();
        
        /// <summary>
        /// Checks if the current user is authenticated
        /// </summary>
        /// <returns>True if authenticated, false otherwise</returns>
        bool IsAuthenticated();
        
        /// <summary>
        /// Checks if the current user is an admin
        /// </summary>
        /// <returns>True if admin, false otherwise</returns>
        bool IsAdmin();
    }
} 