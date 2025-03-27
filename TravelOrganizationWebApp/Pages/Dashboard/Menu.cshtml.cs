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
        public List<AllergenModel> Allergens { get; set; } = new List<AllergenModel>();
        
        public int? SelectedCategoryId { get; set; }
        public string SelectedCategorySlug { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;
        public List<int> ExcludedAllergenIds { get; set; } = new List<int>();

        public async Task<IActionResult> OnGetAsync(string category = "", string searchTerm = "", string excludeAllergens = "")
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            SearchTerm = searchTerm;
            SelectedCategorySlug = category;
            
            // Parse excluded allergen IDs if any
            if (!string.IsNullOrEmpty(excludeAllergens))
            {
                ExcludedAllergenIds = excludeAllergens.Split(',')
                    .Where(id => int.TryParse(id, out _))
                    .Select(int.Parse)
                    .ToList();
            }

            try
            {
                // Load categories
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                
                // Load allergens
                Allergens = (await _foodService.GetAllAllergensAsync()).ToList();
                
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

                // Load all food items by default, or filter by category/search if provided
                if (string.IsNullOrEmpty(searchTerm) && !SelectedCategoryId.HasValue)
                {
                    // Show all food items by default instead of filtering by "Hot Dishes"
                    MenuItems = (await _foodService.GetAllFoodAsync()).ToList();
                }
                else
                {
                    MenuItems = (await _foodService.SearchFoodAsync(searchTerm, SelectedCategoryId)).ToList();
                }
                
                // Filter out foods with excluded allergens
                if (ExcludedAllergenIds.Any())
                {
                    MenuItems = MenuItems
                        .Where(food => !food.Allergens.Any(a => ExcludedAllergenIds.Contains(a.Id)))
                        .ToList();
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