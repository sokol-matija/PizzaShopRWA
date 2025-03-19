using PizzaShopWebApp.Models;

namespace PizzaShopWebApp.Services
{
    public interface IFoodService : IApiService
    {
        Task<IEnumerable<MenuItemModel>> GetAllFoodAsync(int page = 1, int count = 10);
        Task<IEnumerable<MenuItemModel>> GetFoodByCategoryAsync(int categoryId, int page = 1, int count = 10);
        Task<MenuItemModel> GetFoodByIdAsync(int id);
        Task<IEnumerable<MenuItemModel>> SearchFoodAsync(string searchTerm, int? categoryId = null, int page = 1, int count = 10);
        Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync();
        Task<IEnumerable<AllergenModel>> GetAllAllergensAsync();
    }
} 