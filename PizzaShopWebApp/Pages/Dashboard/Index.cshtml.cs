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

        // Sales trends 
        public Dictionary<string, decimal> WeeklySalesTrend { get; set; } = new Dictionary<string, decimal>();
        
        // Order type distribution
        public Dictionary<string, int> OrderTypeDistribution { get; set; } = new Dictionary<string, int>();
        
        // Popular categories
        public Dictionary<string, int> PopularCategories { get; set; } = new Dictionary<string, int>();
        
        // Time of day distribution
        public Dictionary<string, int> OrdersByTimeOfDay { get; set; } = new Dictionary<string, int>();
        
        // Today's performance metrics
        public decimal TodayRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int TodayCustomers { get; set; }
        
        // Order status distribution
        public Dictionary<string, int> OrderStatusDistribution { get; set; } = new Dictionary<string, int>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Get all orders
                var allOrders = await _orderService.GetAllOrdersAsync(1, 100);
                var orders = allOrders.ToList();

                // Get today's and yesterday's orders
                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);
                
                var todayOrders = orders.Where(o => o.OrderDate.Date == today).ToList();
                var yesterdayOrders = orders.Where(o => o.OrderDate.Date == yesterday).ToList();

                // Calculate today's metrics
                TodayRevenue = todayOrders.Sum(o => o.TotalAmount);
                TodayOrders = todayOrders.Count;
                TodayCustomers = todayOrders.Select(o => o.OrderNumber).Distinct().Count();

                // Calculate yesterday's metrics for comparison
                var yesterdayRevenue = yesterdayOrders.Sum(o => o.TotalAmount);
                var yesterdayOrdersCount = yesterdayOrders.Count;
                var yesterdayCustomers = yesterdayOrders.Select(o => o.OrderNumber).Distinct().Count();

                // Calculate percentage changes
                RevenueChangePercent = yesterdayRevenue > 0 
                    ? ((TodayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100 
                    : 0;
                    
                OrdersChangePercent = yesterdayOrdersCount > 0 
                    ? ((TodayOrders - yesterdayOrdersCount) / (decimal)yesterdayOrdersCount) * 100 
                    : 0;
                    
                CustomersChangePercent = yesterdayCustomers > 0 
                    ? ((TodayCustomers - yesterdayCustomers) / (decimal)yesterdayCustomers) * 100 
                    : 0;

                // Get recent orders (last 5)
                RecentOrders = orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList();

                // Calculate weekly sales trend using real data
                WeeklySalesTrend = CalculateWeeklySalesTrend(orders);

                // Calculate food category distribution
                var foodItems = await _foodService.GetAllFoodAsync(1, 100);
                var categories = await _foodService.GetAllCategoriesAsync();
                
                // Calculate category distribution based on ordered items
                var categoryDistribution = new Dictionary<string, int>();
                foreach (var category in categories)
                {
                    var categoryItems = foodItems.Where(f => f.FoodCategoryId == category.Id);
                    var orderCount = orders
                        .SelectMany(o => o.Items)
                        .Count(i => categoryItems.Any(f => f.Id == i.FoodId));
                    categoryDistribution[category.Name] = orderCount;
                }

                // Replace OrderTypeDistribution with CategoryDistribution
                OrderTypeDistribution = categoryDistribution;

                // Calculate order status distribution
                OrderStatusDistribution = orders
                    .GroupBy(o => o.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Calculate orders by time of day
                OrdersByTimeOfDay = CalculateOrdersByTimeOfDay(orders);

                // Get popular items based on actual order data
                var popularItemIds = orders
                    .SelectMany(o => o.Items)
                    .GroupBy(i => i.FoodId)
                    .OrderByDescending(g => g.Sum(i => i.Quantity))
                    .Select(g => g.Key)
                    .Take(5)
                    .ToList();

                MostOrderedItems = foodItems
                    .Where(f => popularItemIds.Contains(f.Id))
                    .Take(5)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
            }

            return Page();
        }

        // Method to calculate weekly sales trend
        private Dictionary<string, decimal> CalculateWeeklySalesTrend(List<Models.OrderModel> orders)
        {
            var result = new Dictionary<string, decimal>();
            var today = DateTime.Today;
            
            // Get days of week abbreviated names
            var daysOfWeek = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            
            // Initialize with zeros for all days
            foreach (var day in daysOfWeek)
            {
                result[day] = 0;
            }
            
            // Go back 6 days from today to get a 7-day window
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var dayName = daysOfWeek[(int)date.DayOfWeek > 0 ? (int)date.DayOfWeek - 1 : 6]; // Adjust for Sunday
                
                var dayRevenue = orders
                    .Where(o => o.OrderDate.Date == date)
                    .Sum(o => o.TotalAmount);
                    
                result[dayName] = dayRevenue;
            }
            
            return result;
        }
        
        // Method to calculate popular categories
        private Dictionary<string, int> CalculatePopularCategories(
            List<Models.OrderModel> orders, 
            List<MenuItemModel> foodItems,
            List<CategoryModel> categories)
        {
            var result = new Dictionary<string, int>();
            
            // Initialize with all categories
            foreach (var category in categories)
            {
                result[category.Name] = 0;
            }
            
            // If no categories found, add some defaults
            if (!result.Any())
            {
                result["Pizza"] = 150;
                result["Pasta"] = 100;
                result["Burgers"] = 80;
                result["Drinks"] = 120;
                result["Desserts"] = 60;
                return result;
            }
            
            // Create lookup of food id to category
            var foodCategoryMap = foodItems.ToDictionary(f => f.Id, f => f.FoodCategoryName);
            
            // Count orders by category
            foreach (var order in orders)
            {
                foreach (var item in order.Items)
                {
                    // If we can map this item to a category
                    if (foodCategoryMap.TryGetValue(item.FoodId, out var categoryName))
                    {
                        if (result.ContainsKey(categoryName))
                        {
                            result[categoryName] += item.Quantity;
                        }
                    }
                }
            }
            
            // Filter to only categories with orders and take top 5
            return result
                .Where(kvp => kvp.Value > 0)
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        
        // Method to calculate orders by time of day
        private Dictionary<string, int> CalculateOrdersByTimeOfDay(List<Models.OrderModel> orders)
        {
            var result = new Dictionary<string, int>
            {
                { "Morning (6-11 AM)", 0 },
                { "Lunch (11-2 PM)", 0 },
                { "Afternoon (2-5 PM)", 0 },
                { "Dinner (5-9 PM)", 0 },
                { "Night (9 PM-6 AM)", 0 }
            };
            
            foreach (var order in orders)
            {
                var hour = order.OrderDate.Hour;
                
                if (hour >= 6 && hour < 11)
                    result["Morning (6-11 AM)"]++;
                else if (hour >= 11 && hour < 14)
                    result["Lunch (11-2 PM)"]++;
                else if (hour >= 14 && hour < 17)
                    result["Afternoon (2-5 PM)"]++;
                else if (hour >= 17 && hour < 21)
                    result["Dinner (5-9 PM)"]++;
                else
                    result["Night (9 PM-6 AM)"]++;
            }
            
            return result;
        }
        
        // Method to calculate order status distribution
        private Dictionary<string, int> CalculateOrderStatusDistribution(List<Models.OrderModel> orders)
        {
            return orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count());
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