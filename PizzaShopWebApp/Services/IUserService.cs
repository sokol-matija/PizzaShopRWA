using PizzaShopWebApp.Models;

namespace PizzaShopWebApp.Services
{
    public interface IUserService : IApiService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(RegisterModel model);
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        UserProfileModel GetUserProfileAsync();
        bool UpdateUserProfileAsync(UserProfileModel profile);
        Task LogoutAsync();
        bool IsAuthenticated();
        bool IsAdmin();
    }
} 