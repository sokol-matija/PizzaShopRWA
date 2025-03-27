using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaShopWebApp.Pages.Orders
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<Order> ActiveOrders { get; set; } = new List<Order>();
        public List<Order> CompletedOrders { get; set; } = new List<Order>();

        public IActionResult OnGet()
        {
            // Check authentication
            var authToken = HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(authToken))
            {
                _logger.LogInformation("User not authenticated, redirecting to login");
                return RedirectToPage("/Account/Login");
            }

            // Load orders from API or database
            // For demo purposes, we'll use sample data
            LoadSampleOrders();
            
            return Page();
        }

        private void LoadSampleOrders()
        {
            // Sample active orders
            ActiveOrders = new List<Order>
            {
                new Order
                {
                    Id = 101,
                    OrderNumber = "ORD-2025-1001",
                    OrderDate = DateTime.Now.AddHours(-2),
                    Status = "In Progress",
                    Total = 35.96m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 1, Name = "Pepperoni Pizza", Quantity = 1, Price = 13.99m },
                        new OrderItem { Id = 2, Name = "Garlic Bread", Quantity = 2, Price = 4.99m },
                        new OrderItem { Id = 3, Name = "Soda (Coke)", Quantity = 3, Price = 2.49m },
                        new OrderItem { Id = 4, Name = "Chocolate Chip Cookie", Quantity = 2, Price = 2.99m }
                    }
                },
                new Order
                {
                    Id = 102,
                    OrderNumber = "ORD-2025-1002",
                    OrderDate = DateTime.Now.AddMinutes(-30),
                    Status = "Placed",
                    Total = 23.97m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 5, Name = "Margherita Pizza", Quantity = 1, Price = 11.99m },
                        new OrderItem { Id = 6, Name = "Caesar Salad", Quantity = 1, Price = 7.99m },
                        new OrderItem { Id = 7, Name = "Bottled Water", Quantity = 2, Price = 1.99m }
                    }
                }
            };

            // Sample completed orders
            CompletedOrders = new List<Order>
            {
                new Order
                {
                    Id = 95,
                    OrderNumber = "ORD-2025-995",
                    OrderDate = DateTime.Now.AddDays(-2),
                    Status = "Completed",
                    Total = 42.95m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 8, Name = "Meat Lovers Pizza", Quantity = 1, Price = 16.99m },
                        new OrderItem { Id = 9, Name = "BBQ Chicken Pizza", Quantity = 1, Price = 15.99m },
                        new OrderItem { Id = 10, Name = "Chicken Wings", Quantity = 1, Price = 8.99m }
                    }
                },
                new Order
                {
                    Id = 92,
                    OrderNumber = "ORD-2025-992",
                    OrderDate = DateTime.Now.AddDays(-5),
                    Status = "Completed",
                    Total = 29.97m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 11, Name = "Hawaiian Pizza", Quantity = 1, Price = 14.99m },
                        new OrderItem { Id = 12, Name = "Mozzarella Sticks", Quantity = 1, Price = 6.99m },
                        new OrderItem { Id = 13, Name = "Brownie", Quantity = 2, Price = 3.99m }
                    }
                },
                new Order
                {
                    Id = 87,
                    OrderNumber = "ORD-2025-987",
                    OrderDate = DateTime.Now.AddDays(-7),
                    Status = "Cancelled",
                    Total = 23.98m,
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Id = 14, Name = "Vegetarian Supreme Pizza", Quantity = 1, Price = 14.99m },
                        new OrderItem { Id = 15, Name = "Garlic Bread", Quantity = 1, Price = 4.99m },
                        new OrderItem { Id = 16, Name = "Iced Tea", Quantity = 2, Price = 2.00m }
                    }
                }
            };
        }
    }

    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
} 