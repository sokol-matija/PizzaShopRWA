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
        public string SearchTerm { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? categoryId = null, string searchTerm = "")
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            SelectedCategoryId = categoryId;
            SearchTerm = searchTerm;

            try
            {
                // Load categories
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                
                // Load all food items
                if (string.IsNullOrEmpty(searchTerm) && !categoryId.HasValue)
                {
                    MenuItems = (await _foodService.GetAllFoodAsync()).ToList();
                }
                else
                {
                    MenuItems = (await _foodService.SearchFoodAsync(searchTerm, categoryId)).ToList();
                }

                // Group food items by category
                foreach (var category in Categories)
                {
                    var foodsInCategory = MenuItems
                        .Where(f => f.FoodCategoryId == category.Id)
                        .ToList();
                    
                    FoodByCategory[category.Id] = foodsInCategory;
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