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
                // Get recent orders (last 5 orders)
                var orders = await _orderService.GetUserOrdersAsync();
                RecentOrders = orders.OrderByDescending(o => o.OrderDate).Take(5).ToList();

                // Calculate stats from orders (in a real app, these would come from the API)
                TotalRevenue = orders.Sum(o => o.TotalAmount);
                TotalDishesOrdered = orders.Sum(o => o.Items.Sum(i => i.Quantity));
                TotalCustomers = 100; // Placeholder - would come from API

                // Set placeholder change percentages (in a real app, these would be calculated)
                RevenueChangePercent = 32.40m;
                OrdersChangePercent = -12.40m;
                CustomersChangePercent = 2.40m;

                // Get popular food items
                var allFoodItems = await _foodService.GetAllFoodAsync(1, 100);
                // In a real app, this would be sorted by popularity from the API
                // Here we're just taking the first 3 items as an example
                MostOrderedItems = allFoodItems.Take(3).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                // Don't redirect, just show the page with empty data
                // We could add a notification to the UI about the error
            }

            return Page();
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