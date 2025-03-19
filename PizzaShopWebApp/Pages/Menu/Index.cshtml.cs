using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaShopWebApp.Models;
using PizzaShopWebApp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PizzaShopWebApp.Pages.Menu
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IFoodService _foodService;
        private readonly IUserService _userService;

        public IndexModel(
            ILogger<IndexModel> logger,
            IFoodService foodService,
            IUserService userService)
        {
            _logger = logger;
            _foodService = foodService;
            _userService = userService;
        }

        public List<MenuItemModel> Pizzas { get; set; } = new List<MenuItemModel>();
        public List<MenuItemModel> Sides { get; set; } = new List<MenuItemModel>();
        public List<MenuItemModel> Drinks { get; set; } = new List<MenuItemModel>();
        public List<MenuItemModel> Desserts { get; set; } = new List<MenuItemModel>();
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check authentication
            if (!_userService.IsAuthenticated())
            {
                _logger.LogInformation("User not authenticated, redirecting to login");
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Load categories
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                
                // Define category IDs for each food type
                // These should match your actual category IDs in the database
                var pizzaCategoryId = Categories.FirstOrDefault(c => c.Name.Contains("Pizza"))?.Id ?? 1;
                var sidesCategoryId = Categories.FirstOrDefault(c => c.Name.Contains("Burger") || c.Name.Contains("Side"))?.Id ?? 2;
                var drinksCategoryId = Categories.FirstOrDefault(c => c.Name.Contains("Drink") || c.Name.Contains("Beverage"))?.Id ?? 5;
                var dessertsCategoryId = Categories.FirstOrDefault(c => c.Name.Contains("Dessert"))?.Id ?? 5;
                
                // Load food by categories
                var pizzaTask = _foodService.GetFoodByCategoryAsync(pizzaCategoryId);
                var sidesTask = _foodService.GetFoodByCategoryAsync(sidesCategoryId);
                var drinksTask = _foodService.GetFoodByCategoryAsync(drinksCategoryId);
                var dessertsTask = _foodService.GetFoodByCategoryAsync(dessertsCategoryId);
                
                // Wait for all tasks to complete
                await Task.WhenAll(pizzaTask, sidesTask, drinksTask, dessertsTask);
                
                // Assign results
                Pizzas = pizzaTask.Result.ToList();
                Sides = sidesTask.Result.ToList();
                Drinks = drinksTask.Result.ToList();
                Desserts = dessertsTask.Result.ToList();
                
                // If no data was retrieved, use sample data as fallback
                if (!Pizzas.Any() && !Sides.Any() && !Drinks.Any() && !Desserts.Any())
                {
                    _logger.LogWarning("No food data retrieved from API, using sample data as fallback");
                    LoadSampleData();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu data");
                LoadSampleData();
            }
            
            return Page();
        }
        
        // Keep the sample data method as fallback
        private void LoadSampleData()
        {
            // Sample pizzas
            Pizzas = new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    Id = 1,
                    Name = "Margherita",
                    Description = "Classic pizza with tomato sauce, mozzarella, and fresh basil",
                    Price = 11.99m,
                    ImageUrl = "/images/food/margherita.jpg",
                    IsVegetarian = true,
                    IsPopular = true
                },
                new MenuItemModel
                {
                    Id = 2,
                    Name = "Pepperoni",
                    Description = "Traditional pizza topped with pepperoni slices and mozzarella",
                    Price = 13.99m,
                    ImageUrl = "/images/food/pepperoni.jpg",
                    IsPopular = true
                },
                new MenuItemModel
                {
                    Id = 3,
                    Name = "Vegetarian Supreme",
                    Description = "Loaded with bell peppers, mushrooms, olives, onions, and tomatoes",
                    Price = 14.99m,
                    ImageUrl = "/images/food/vegetarian.jpg",
                    IsVegetarian = true
                },
                new MenuItemModel
                {
                    Id = 4,
                    Name = "Meat Lovers",
                    Description = "Pepperoni, sausage, bacon, ham, and ground beef on a traditional crust",
                    Price = 16.99m,
                    ImageUrl = "/images/food/meat-lovers.jpg"
                },
                new MenuItemModel
                {
                    Id = 5,
                    Name = "Hawaiian",
                    Description = "Ham, pineapple, and mozzarella cheese on a traditional crust",
                    Price = 14.99m,
                    ImageUrl = "/images/food/hawaiian.jpg"
                },
                new MenuItemModel
                {
                    Id = 6,
                    Name = "BBQ Chicken",
                    Description = "Grilled chicken, red onions, and BBQ sauce with mozzarella cheese",
                    Price = 15.99m,
                    ImageUrl = "/images/food/bbq-chicken.jpg"
                }
            };
            
            // Sample sides
            Sides = new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    Id = 101,
                    Name = "Garlic Bread",
                    Description = "Freshly baked bread with garlic butter and herbs",
                    Price = 4.99m,
                    ImageUrl = "/images/food/garlic-bread.jpg",
                    IsVegetarian = true,
                    IsPopular = true
                },
                new MenuItemModel
                {
                    Id = 102,
                    Name = "Chicken Wings",
                    Description = "Crispy chicken wings with your choice of sauce (BBQ, Buffalo, or Plain)",
                    Price = 8.99m,
                    ImageUrl = "/images/food/chicken-wings.jpg"
                },
                new MenuItemModel
                {
                    Id = 103,
                    Name = "Mozzarella Sticks",
                    Description = "Breaded and fried mozzarella sticks with marinara sauce",
                    Price = 6.99m,
                    ImageUrl = "/images/food/mozzarella-sticks.jpg",
                    IsVegetarian = true
                },
                new MenuItemModel
                {
                    Id = 104,
                    Name = "Caesar Salad",
                    Description = "Fresh romaine lettuce, croutons, parmesan cheese, and Caesar dressing",
                    Price = 7.99m,
                    ImageUrl = "/images/food/caesar-salad.jpg",
                    IsVegetarian = true
                }
            };
            
            // Sample drinks
            Drinks = new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    Id = 201,
                    Name = "Soda",
                    Description = "Your choice of Coke, Diet Coke, Sprite, or Root Beer (20 oz)",
                    Price = 2.49m,
                    ImageUrl = "/images/food/soda.jpg"
                },
                new MenuItemModel
                {
                    Id = 202,
                    Name = "Iced Tea",
                    Description = "Freshly brewed iced tea, sweetened or unsweetened (20 oz)",
                    Price = 2.49m,
                    ImageUrl = "/images/food/iced-tea.jpg"
                },
                new MenuItemModel
                {
                    Id = 203,
                    Name = "Bottled Water",
                    Description = "Purified bottled water (16.9 oz)",
                    Price = 1.99m,
                    ImageUrl = "/images/food/water.jpg"
                },
                new MenuItemModel
                {
                    Id = 204,
                    Name = "Craft Beer",
                    Description = "Local IPA, Lager, or Stout (must be 21+ to purchase)",
                    Price = 5.99m,
                    ImageUrl = "/images/food/beer.jpg"
                }
            };
            
            // Sample desserts
            Desserts = new List<MenuItemModel>
            {
                new MenuItemModel
                {
                    Id = 301,
                    Name = "Chocolate Chip Cookie",
                    Description = "Large, freshly baked chocolate chip cookie",
                    Price = 2.99m,
                    ImageUrl = "/images/food/chocolate-chip-cookie.jpg",
                    IsVegetarian = true,
                    IsPopular = true
                },
                new MenuItemModel
                {
                    Id = 302,
                    Name = "Brownie",
                    Description = "Rich, fudgy chocolate brownie with walnuts",
                    Price = 3.99m,
                    ImageUrl = "/images/food/brownie.jpg",
                    IsVegetarian = true
                },
                new MenuItemModel
                {
                    Id = 303,
                    Name = "Cinnamon Sticks",
                    Description = "Sweet sticks with cinnamon sugar, served with icing dip",
                    Price = 5.99m,
                    ImageUrl = "/images/food/cinnamon-sticks.jpg",
                    IsVegetarian = true
                }
            };
        }
    }
} 