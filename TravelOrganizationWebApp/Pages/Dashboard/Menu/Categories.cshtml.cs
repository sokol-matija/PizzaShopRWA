using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaShopWebApp.Models;
using PizzaShopWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShopWebApp.Pages.Dashboard.Menu
{
    public class CategoriesModel : PageModel
    {
        private readonly IFoodService _foodService;
        private readonly IUserService _userService;
        private readonly ILogger<CategoriesModel> _logger;

        public CategoriesModel(
            IFoodService foodService,
            IUserService userService,
            ILogger<CategoriesModel> logger)
        {
            _foodService = foodService;
            _userService = userService;
            _logger = logger;
        }

        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public Dictionary<int, int> ItemsCountByCategory { get; set; } = new Dictionary<int, int>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Load categories
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                
                // For each category, count the number of items
                var allItems = await _foodService.GetAllFoodAsync();
                
                foreach (var category in Categories)
                {
                    ItemsCountByCategory[category.Id] = allItems.Count(i => i.FoodCategoryId == category.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories data");
                TempData["ErrorMessage"] = "Failed to load categories. Please try again later.";
            }

            return Page();
        }
        
        public IActionResult OnPostAsync(int categoryId, string categoryName, string categoryDescription)
        {
            try
            {
                // In a real application, you would call your service to create or update the category
                // For this example, we're just showing success messages
                
                if (categoryId == 0)
                {
                    // Create new category
                    TempData["SuccessMessage"] = $"Category '{categoryName}' created successfully.";
                }
                else
                {
                    // Update existing category
                    TempData["SuccessMessage"] = $"Category '{categoryName}' updated successfully.";
                }
                
                // Redirect to GET to refresh the data
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving category");
                TempData["ErrorMessage"] = "Failed to save category. Please try again.";
                return RedirectToPage();
            }
        }
        
        public IActionResult OnPostDeleteAsync(int categoryId)
        {
            try
            {
                // In a real application, you would call your service to delete the category
                // For this example, we're just showing a success message
                
                TempData["SuccessMessage"] = "Category deleted successfully.";
                
                // Redirect to GET to refresh the data
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                TempData["ErrorMessage"] = "Failed to delete category. Please try again.";
                return RedirectToPage();
            }
        }
    }
} 