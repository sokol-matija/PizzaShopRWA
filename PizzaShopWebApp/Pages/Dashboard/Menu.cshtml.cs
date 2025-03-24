using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaShopWebApp.Models;
using PizzaShopWebApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShopWebApp.Pages.Dashboard
{
    public class MenuModel : PageModel
    {
        private readonly IFoodService _foodService;
        private readonly IUserService _userService;
        private readonly ILogger<MenuModel> _logger;

        public MenuModel(
            IFoodService foodService,
            IUserService userService,
            ILogger<MenuModel> logger)
        {
            _foodService = foodService;
            _userService = userService;
            _logger = logger;
        }

        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public List<MenuItemModel> MenuItems { get; set; } = new List<MenuItemModel>();
        public Dictionary<int, List<MenuItemModel>> FoodByCategory { get; set; } = new Dictionary<int, List<MenuItemModel>>();
        
        public int? SelectedCategoryId { get; set; }
        public string SelectedCategorySlug { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(string category = "", string searchTerm = "")
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            SearchTerm = searchTerm;
            SelectedCategorySlug = category;

            try
            {
                // Load categories
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                
                // If we have a category slug, find the matching category ID
                if (!string.IsNullOrEmpty(category))
                {
                    var matchingCategory = Categories.FirstOrDefault(c => 
                        c.Name.ToLower().Replace(" ", "-") == category);
                    
                    if (matchingCategory != null)
                    {
                        SelectedCategoryId = matchingCategory.Id;
                    }
                }

                // Load all food items
                if (string.IsNullOrEmpty(searchTerm) && !SelectedCategoryId.HasValue)
                {
                    // If no category is selected, default to 'Hot Dishes' or the first category
                    var hotDishesCategory = Categories.FirstOrDefault(c => c.Name == "Hot Dishes");
                    if (hotDishesCategory != null)
                    {
                        MenuItems = (await _foodService.GetFoodByCategoryAsync(hotDishesCategory.Id)).ToList();
                    }
                    else if (Categories.Any())
                    {
                        // Fall back to first category if 'Hot Dishes' doesn't exist
                        MenuItems = (await _foodService.GetFoodByCategoryAsync(Categories.First().Id)).ToList();
                    }
                    else
                    {
                        // If no categories, show all food
                        MenuItems = (await _foodService.GetAllFoodAsync()).ToList();
                    }
                }
                else
                {
                    MenuItems = (await _foodService.SearchFoodAsync(searchTerm, SelectedCategoryId)).ToList();
                }

                // Group food items by category
                foreach (var cat in Categories)
                {
                    var foodsInCategory = MenuItems
                        .Where(f => f.FoodCategoryId == cat.Id)
                        .ToList();
                    
                    FoodByCategory[cat.Id] = foodsInCategory;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu data");
                // We'll just show an empty menu if there's an error
            }

            return Page();
        }
    }
} 