using System.Net.Http.Json;
using PizzaShopWebApp.Models;

namespace PizzaShopWebApp.Services
{
    public class FoodService : ApiServiceBase, IFoodService
    {
        public FoodService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<FoodService> logger) 
            : base(httpClientFactory, httpContextAccessor, configuration, logger)
        {
        }

        public async Task<IEnumerable<MenuItemModel>> GetAllFoodAsync(int page = 1, int count = 10)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.GetAsync($"/api/food?page={page}&count={count}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PagedResultModel<MenuItemModel>>();
                    return result?.Items ?? new List<MenuItemModel>();
                }
                
                _logger.LogWarning("Failed to get food items. Status code: {StatusCode}", response.StatusCode);
                return new List<MenuItemModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all food items");
                return new List<MenuItemModel>();
            }
        }

        public async Task<IEnumerable<MenuItemModel>> GetFoodByCategoryAsync(int categoryId, int page = 1, int count = 10)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.GetAsync($"/api/food/search?categoryId={categoryId}&page={page}&count={count}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PagedResultModel<MenuItemModel>>();
                    return result?.Items ?? new List<MenuItemModel>();
                }
                
                _logger.LogWarning("Failed to get food items by category. Status code: {StatusCode}", response.StatusCode);
                return new List<MenuItemModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting food items by category");
                return new List<MenuItemModel>();
            }
        }

        public async Task<MenuItemModel> GetFoodByIdAsync(int id)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.GetAsync($"/api/food/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<MenuItemModel>();
                    return result ?? new MenuItemModel();
                }
                
                _logger.LogWarning("Failed to get food item by id. Status code: {StatusCode}", response.StatusCode);
                return new MenuItemModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting food item by id");
                return new MenuItemModel();
            }
        }

        public async Task<IEnumerable<MenuItemModel>> SearchFoodAsync(string searchTerm, int? categoryId = null, int page = 1, int count = 10)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var url = $"/api/food/search?page={page}&count={count}";
                
                if (!string.IsNullOrEmpty(searchTerm))
                    url += $"&name={Uri.EscapeDataString(searchTerm)}";
                    
                if (categoryId.HasValue)
                    url += $"&categoryId={categoryId.Value}";
                
                var response = await client.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PagedResultModel<MenuItemModel>>();
                    return result?.Items ?? new List<MenuItemModel>();
                }
                
                _logger.LogWarning("Failed to search food items. Status code: {StatusCode}", response.StatusCode);
                return new List<MenuItemModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching food items");
                return new List<MenuItemModel>();
            }
        }

        public async Task<IEnumerable<CategoryModel>> GetAllCategoriesAsync()
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.GetAsync("/api/foodcategory");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<CategoryModel>>();
                    return result ?? new List<CategoryModel>();
                }
                
                _logger.LogWarning("Failed to get categories. Status code: {StatusCode}", response.StatusCode);
                return new List<CategoryModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return new List<CategoryModel>();
            }
        }

        public async Task<IEnumerable<AllergenModel>> GetAllAllergensAsync()
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.GetAsync("/api/allergen");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<AllergenModel>>();
                    return result ?? new List<AllergenModel>();
                }
                
                _logger.LogWarning("Failed to get allergens. Status code: {StatusCode}", response.StatusCode);
                return new List<AllergenModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all allergens");
                return new List<AllergenModel>();
            }
        }
    }
} 