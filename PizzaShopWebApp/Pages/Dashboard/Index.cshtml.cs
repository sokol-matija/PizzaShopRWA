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
    public class DashboardIndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IFoodService _foodService;
        private readonly IUserService _userService;
        private readonly ILogger<DashboardIndexModel> _logger;

        public DashboardIndexModel(
            IOrderService orderService,
            IFoodService foodService,
            IUserService userService,
            ILogger<DashboardIndexModel> logger)
        {
            _orderService = orderService;
            _foodService = foodService;
            _userService = userService;
            _logger = logger;
        }

        // Order report data
        public List<Models.OrderModel> RecentOrders { get; set; } = new List<Models.OrderModel>();
        public List<Models.OrderModel> AllOrders { get; set; } = new List<Models.OrderModel>();
        
        // Stats data
        public decimal TotalRevenue { get; set; }
        public int TotalDishesOrdered { get; set; }
        public int TotalCustomers { get; set; }
        public decimal RevenueChangePercent { get; set; }
        public decimal OrdersChangePercent { get; set; }
        public decimal CustomersChangePercent { get; set; }
        
        // Most ordered items
        public List<MenuItemModel> MostOrderedItems { get; set; } = new List<MenuItemModel>();
        
        // Current date 
        public string CurrentDate { get; set; } = DateTime.Now.ToString("dddd d MMM, yyyy");

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Log API calls to console
                Console.WriteLine("\n=== Testing API Calls on Dashboard Page ===");
                
                // Get all user orders
                Console.WriteLine("\nGetting user orders from API...");
                var orders = await _orderService.GetUserOrdersAsync();
                Console.WriteLine($"Retrieved {orders.Count()} user orders");
                RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5).ToList();
                AllOrders = orders.ToList();

                // Get all orders (admin only)
                Console.WriteLine("\nGetting all orders from API (admin only)...");
                var allOrders = await _orderService.GetAllOrdersAsync(1, 100);
                Console.WriteLine($"Retrieved {allOrders.Count()} total orders in the system");
                
                // Get popular food items
                Console.WriteLine("\nGetting all food items from API...");
                var allFoodItems = await _foodService.GetAllFoodAsync(1, 100);
                Console.WriteLine($"Retrieved {allFoodItems.Count()} food items");
                
                // Display summary of total sales
                CalculateAndDisplayOrderSummary(allOrders.ToList());

                // Calculate stats from orders
                TotalRevenue = allOrders.Sum(o => o.TotalAmount);
                TotalDishesOrdered = allOrders.Sum(o => o.Items.Sum(i => i.Quantity));
                TotalCustomers = 100; // Placeholder - would come from API

                // Set placeholder change percentages
                RevenueChangePercent = 32.40m;
                OrdersChangePercent = -12.40m;
                CustomersChangePercent = 2.40m;

                // Get popular food items (taking first 3 as an example)
                MostOrderedItems = allFoodItems.Take(3).ToList();
                
                Console.WriteLine("\n=== API Testing Complete ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
                // Don't redirect, just show the page with empty data
            }

            return Page();
        }

        // Method to calculate and display order summary statistics
        private void CalculateAndDisplayOrderSummary(List<Models.OrderModel> orders)
        {
            Console.WriteLine("\n=== Order Summary Statistics ===");
            
            // Calculate total revenue
            decimal totalSales = orders.Sum(o => o.TotalAmount);
            Console.WriteLine($"Total Sales Amount: ${totalSales:F2}");
            
            // Calculate sales by status
            var salesByStatus = orders
                .GroupBy(o => o.Status)
                .Select(g => new { 
                    Status = g.Key, 
                    Count = g.Count(), 
                    Amount = g.Sum(o => o.TotalAmount) 
                })
                .OrderByDescending(x => x.Amount);
                
            Console.WriteLine("\nSales by Order Status:");
            foreach (var status in salesByStatus)
            {
                Console.WriteLine($"- {status.Status}: {status.Count} orders, ${status.Amount:F2}");
            }
            
            // Calculate most ordered items
            var mostOrderedItems = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => i.FoodName)
                .Select(g => new {
                    FoodName = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Price * i.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5);
                
            Console.WriteLine("\nTop 5 Most Ordered Items:");
            foreach (var item in mostOrderedItems)
            {
                Console.WriteLine($"- {item.FoodName}: {item.Quantity} ordered, ${item.Revenue:F2}");
            }
            
            Console.WriteLine("\nNote: These calculations are done client-side by processing the API data");
            Console.WriteLine("In a production environment, these aggregations would typically be handled server-side");
        }

        // Helper method to get appropriate status class for CSS
        public string GetStatusClass(string status)
        {
            return status.ToLower() switch
            {
                "completed" => "completed",
                "delivered" => "completed",
                "accepted" => "preparing",
                "preparing" => "preparing",
                "in progress" => "preparing",
                "pending" => "pending",
                "cancelled" => "cancelled",
                _ => "pending"
            };
        }
    }
} 