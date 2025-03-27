using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    [System.Obsolete("This interface is deprecated. Use destination and trip service interfaces instead.")]
    public interface IFoodService
    {
        Task<IEnumerable<MenuItemModel>> GetAllFoodAsync(int page = 1, int count = 10);
        Task<IEnumerable<MenuItemModel>> GetFoodByCategoryAsync(int categoryId, int page = 1, int count = 10);
        Task<MenuItemModel> GetFoodByIdAsync(int id);
        Task<IEnumerable<MenuItemModel>> SearchFoodAsync(string searchTerm, int? categoryId = null, int page = 1, int count = 10);
        Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync();
        Task<IEnumerable<AllergenModel>> GetAllAllergensAsync();
        
        // Add missing methods for CRUD operations
        Task<MenuItemModel> CreateFoodAsync(MenuItemModel menuItem);
        Task<MenuItemModel> UpdateFoodAsync(MenuItemModel menuItem);
        Task<bool> DeleteFoodAsync(int id);
    }
} 