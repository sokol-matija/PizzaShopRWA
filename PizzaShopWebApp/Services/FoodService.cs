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
        
        public async Task<MenuItemModel> CreateFoodAsync(MenuItemModel menuItem)
        {
            try
            {
                // Handle ImageUrl - ensure it's a valid URL
                string imageUrl = null;
                if (!string.IsNullOrWhiteSpace(menuItem.ImageUrl))
                {
                    // If it's already a full URL, use it as is
                    if (menuItem.ImageUrl.StartsWith("http://") || menuItem.ImageUrl.StartsWith("https://"))
                    {
                        imageUrl = menuItem.ImageUrl;
                    }
                    // If it's a local wwwroot path, use it as is
                    else if (menuItem.ImageUrl.StartsWith("/"))
                    {
                        imageUrl = menuItem.ImageUrl;
                    }
                    // If it's just a filename, assume it's in the food folder
                    else
                    {
                        imageUrl = $"/images/food/{menuItem.ImageUrl}";
                    }
                }

                // Create a request object that EXACTLY matches the API's expected format
                var requestObj = new
                {
                    name = menuItem.Name,
                    description = menuItem.Description ?? string.Empty,
                    price = menuItem.Price,
                    imageUrl = imageUrl ?? "/images/food/default_dummy.png", // Use local default image if none provided
                    preparationTime = menuItem.PreparationTime,
                    foodCategoryId = menuItem.FoodCategoryId,
                    allergenIds = menuItem.Allergens?.Select(a => a.Id).ToArray() ?? Array.Empty<int>()
                };

                // For debugging - log the request
                _logger.LogInformation($"Sending create request: {System.Text.Json.JsonSerializer.Serialize(requestObj)}");

                var client = await GetHttpClientAsync();
                
                // Make sure we're using the exact casing and format expected by the API
                var response = await client.PostAsJsonAsync("/api/Food", requestObj);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<MenuItemModel>();
                    return result ?? new MenuItemModel();
                }
                
                _logger.LogWarning("Failed to create food item. Status code: {StatusCode}", response.StatusCode);
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                
                throw new Exception($"Failed to create food item: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating food item");
                throw;
            }
        }
        
        public async Task<MenuItemModel> UpdateFoodAsync(MenuItemModel menuItem)
        {
            try
            {
                // Handle ImageUrl - ensure it's a valid URL
                string imageUrl = null;
                if (!string.IsNullOrWhiteSpace(menuItem.ImageUrl))
                {
                    // If it's already a full URL, use it as is
                    if (menuItem.ImageUrl.StartsWith("http://") || menuItem.ImageUrl.StartsWith("https://"))
                    {
                        imageUrl = menuItem.ImageUrl;
                    }
                    // If it's a local wwwroot path, use it as is
                    else if (menuItem.ImageUrl.StartsWith("/"))
                    {
                        imageUrl = menuItem.ImageUrl;
                    }
                    // If it's just a filename, assume it's in the food folder
                    else
                    {
                        imageUrl = $"/images/food/{menuItem.ImageUrl}";
                    }
                }

                // Create a request object that EXACTLY matches the API's expected format
                var requestObj = new
                {
                    name = menuItem.Name,
                    description = menuItem.Description ?? string.Empty,
                    price = menuItem.Price,
                    imageUrl = imageUrl ?? "/images/food/default_dummy.png", // Use local default image if none provided
                    preparationTime = menuItem.PreparationTime,
                    foodCategoryId = menuItem.FoodCategoryId,
                    allergenIds = menuItem.Allergens?.Select(a => a.Id).ToArray() ?? Array.Empty<int>()
                };

                // For debugging - log the request
                _logger.LogInformation($"Sending update request for food ID {menuItem.Id}: {System.Text.Json.JsonSerializer.Serialize(requestObj)}");

                var client = await GetHttpClientAsync();
                
                // Make sure we're using the exact casing and format expected by the API
                var response = await client.PutAsJsonAsync($"/api/Food/{menuItem.Id}", requestObj);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<MenuItemModel>();
                    return result ?? menuItem;
                }
                
                _logger.LogWarning("Failed to update food item. Status code: {StatusCode}", response.StatusCode);
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                
                throw new Exception($"Failed to update food item: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating food item");
                throw;
            }
        }
        
        public async Task<bool> DeleteFoodAsync(int id)
        {
            try
            {
                var client = await GetHttpClientAsync();
                // Note: Using uppercase 'F' in Food to match the API
                var response = await client.DeleteAsync($"/api/Food/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                _logger.LogWarning("Failed to delete food item. Status code: {StatusCode}", response.StatusCode);
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Error content: {ErrorContent}", errorContent);
                
                throw new Exception($"Failed to delete food item: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting food item");
                throw;
            }
        }
    }
} 