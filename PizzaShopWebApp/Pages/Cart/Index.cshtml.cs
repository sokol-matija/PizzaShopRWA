using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace PizzaShopWebApp.Pages.Cart
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal Subtotal => CartItems.Sum(item => item.Price * item.Quantity);
        public decimal Tax => Subtotal * 0.08m; // 8% tax
        public decimal DeliveryFee => 3.99m;
        public decimal Total => Subtotal + Tax + DeliveryFee;

        public IActionResult OnGet()
        {
            // Check authentication
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                _logger.LogInformation("User not authenticated, redirecting to login");
                return RedirectToPage("/Account/Login");
            }

            // Load cart items from session or API
            // In a real application, this would come from your API or database
            // For now, we'll use sample data
            LoadSampleCartData();
            
            return Page();
        }
        
        public IActionResult OnPostCheckout()
        {
            // Process checkout logic would go here
            // Redirect to a checkout page or process payment
            // For now, just redirect back to the cart
            return RedirectToPage();
        }
        
        private void LoadSampleCartData()
        {
            CartItems = new List<CartItem>
            {
                new CartItem
                {
                    Id = 1,
                    Name = "Margherita",
                    Description = "Classic pizza with tomato sauce, mozzarella, and fresh basil",
                    Price = 11.99m,
                    ImageUrl = "/images/food/margherita.jpg",
                    Quantity = 1
                },
                new CartItem
                {
                    Id = 102,
                    Name = "Chicken Wings",
                    Description = "Crispy chicken wings with your choice of sauce",
                    Price = 8.99m,
                    ImageUrl = "/images/food/chicken-wings.jpg",
                    Quantity = 2,
                    Customizations = "Buffalo sauce"
                },
                new CartItem
                {
                    Id = 201,
                    Name = "Soda",
                    Description = "Your choice of Coke, Diet Coke, Sprite, or Root Beer",
                    Price = 2.49m,
                    ImageUrl = "/images/food/soda.jpg",
                    Quantity = 3,
                    Customizations = "Coke"
                }
            };
        }
    }
    
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Customizations { get; set; }
    }
} 