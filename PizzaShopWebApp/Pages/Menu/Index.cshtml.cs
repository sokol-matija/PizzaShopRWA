using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace PizzaShopWebApp.Pages.Menu
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<MenuItem> Pizzas { get; set; } = new List<MenuItem>();
        public List<MenuItem> Sides { get; set; } = new List<MenuItem>();
        public List<MenuItem> Drinks { get; set; } = new List<MenuItem>();
        public List<MenuItem> Desserts { get; set; } = new List<MenuItem>();

        public IActionResult OnGet()
        {
            // Check authentication
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                _logger.LogInformation("User not authenticated, redirecting to login");
                return RedirectToPage("/Account/Login");
            }

            // Initialize with sample data
            // In a real application, this would come from an API or database
            LoadSampleData();
            
            return Page();
        }
        
        private void LoadSampleData()
        {
            // Sample pizzas
            Pizzas = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 1,
                    Name = "Margherita",
                    Description = "Classic pizza with tomato sauce, mozzarella, and fresh basil",
                    Price = 11.99m,
                    ImageUrl = "/images/food/margherita.jpg",
                    IsVegetarian = true,
                    IsPopular = true
                },
                new MenuItem
                {
                    Id = 2,
                    Name = "Pepperoni",
                    Description = "Traditional pizza topped with pepperoni slices and mozzarella",
                    Price = 13.99m,
                    ImageUrl = "/images/food/pepperoni.jpg",
                    IsPopular = true
                },
                new MenuItem
                {
                    Id = 3,
                    Name = "Vegetarian Supreme",
                    Description = "Loaded with bell peppers, mushrooms, olives, onions, and tomatoes",
                    Price = 14.99m,
                    ImageUrl = "/images/food/vegetarian.jpg",
                    IsVegetarian = true
                },
                new MenuItem
                {
                    Id = 4,
                    Name = "Meat Lovers",
                    Description = "Pepperoni, sausage, bacon, ham, and ground beef on a traditional crust",
                    Price = 16.99m,
                    ImageUrl = "/images/food/meat-lovers.jpg"
                },
                new MenuItem
                {
                    Id = 5,
                    Name = "Hawaiian",
                    Description = "Ham, pineapple, and mozzarella cheese on a traditional crust",
                    Price = 14.99m,
                    ImageUrl = "/images/food/hawaiian.jpg"
                },
                new MenuItem
                {
                    Id = 6,
                    Name = "BBQ Chicken",
                    Description = "Grilled chicken, red onions, and BBQ sauce with mozzarella cheese",
                    Price = 15.99m,
                    ImageUrl = "/images/food/bbq-chicken.jpg"
                }
            };
            
            // Sample sides
            Sides = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 101,
                    Name = "Garlic Bread",
                    Description = "Freshly baked bread with garlic butter and herbs",
                    Price = 4.99m,
                    ImageUrl = "/images/food/garlic-bread.jpg",
                    IsVegetarian = true,
                    IsPopular = true
                },
                new MenuItem
                {
                    Id = 102,
                    Name = "Chicken Wings",
                    Description = "Crispy chicken wings with your choice of sauce (BBQ, Buffalo, or Plain)",
                    Price = 8.99m,
                    ImageUrl = "/images/food/chicken-wings.jpg"
                },
                new MenuItem
                {
                    Id = 103,
                    Name = "Mozzarella Sticks",
                    Description = "Breaded and fried mozzarella sticks with marinara sauce",
                    Price = 6.99m,
                    ImageUrl = "/images/food/mozzarella-sticks.jpg",
                    IsVegetarian = true
                },
                new MenuItem
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
            Drinks = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 201,
                    Name = "Soda",
                    Description = "Your choice of Coke, Diet Coke, Sprite, or Root Beer (20 oz)",
                    Price = 2.49m,
                    ImageUrl = "/images/food/soda.jpg"
                },
                new MenuItem
                {
                    Id = 202,
                    Name = "Iced Tea",
                    Description = "Freshly brewed iced tea, sweetened or unsweetened (20 oz)",
                    Price = 2.49m,
                    ImageUrl = "/images/food/iced-tea.jpg"
                },
                new MenuItem
                {
                    Id = 203,
                    Name = "Bottled Water",
                    Description = "Purified bottled water (16.9 oz)",
                    Price = 1.99m,
                    ImageUrl = "/images/food/water.jpg"
                },
                new MenuItem
                {
                    Id = 204,
                    Name = "Craft Beer",
                    Description = "Local IPA, Lager, or Stout (must be 21+ to purchase)",
                    Price = 5.99m,
                    ImageUrl = "/images/food/beer.jpg"
                }
            };
            
            // Sample desserts
            Desserts = new List<MenuItem>
            {
                new MenuItem
                {
                    Id = 301,
                    Name = "Chocolate Chip Cookie",
                    Description = "Large, freshly baked chocolate chip cookie",
                    Price = 2.99m,
                    ImageUrl = "/images/food/chocolate-chip-cookie.jpg",
                    IsVegetarian = true,
                    IsPopular = true
                },
                new MenuItem
                {
                    Id = 302,
                    Name = "Brownie",
                    Description = "Rich, fudgy chocolate brownie with walnuts",
                    Price = 3.99m,
                    ImageUrl = "/images/food/brownie.jpg",
                    IsVegetarian = true
                },
                new MenuItem
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
    
    // Menu item model class
    public class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsVegetarian { get; set; }
        public bool IsPopular { get; set; }
    }
} 