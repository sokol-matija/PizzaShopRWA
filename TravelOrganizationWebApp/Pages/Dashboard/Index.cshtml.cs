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
    // Define the date filter types
    public enum DateFilterType
    {
        Today,
        Week,
        Month,
        Year,
        AllTime
    }

    public class DashboardIndexModel : PageModel
    {
        private readonly IOrderService _orderService;
        private readonly IFoodService _foodService;
        private readonly IUserService _userService;
        private readonly ILogger<DashboardIndexModel> _logger;

        // Date filter properties
        [BindProperty(SupportsGet = true)]
        public DateFilterType DateFilter { get; set; } = DateFilterType.Today;
        public DateTime FilterStartDate { get; private set; }
        public DateTime FilterEndDate { get; private set; }

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
                // Set date range based on the filter selection
                CalculateDateRange();
                
                // Get all orders
                var allOrders = await _orderService.GetAllOrdersAsync(1, 1000);
                var orders = allOrders.ToList();
                
                // Filter orders based on selected date range
                var filteredOrders = orders.Where(o => o.OrderDate >= FilterStartDate && o.OrderDate <= FilterEndDate).ToList();

                // Calculate metrics for the filtered period
                TodayRevenue = filteredOrders.Sum(o => o.TotalAmount);
                TodayOrders = filteredOrders.Count;
                
                // Count distinct customers - first by name, then fallback to id if name is missing
                TodayCustomers = filteredOrders
                    .Where(o => !string.IsNullOrEmpty(o.CustomerName))
                    .Select(o => o.CustomerName)
                    .Distinct()
                    .Count();
                
                // If no customers with names were found, fallback to counting by ID
                if (TodayCustomers == 0)
                {
                    TodayCustomers = filteredOrders.Select(o => o.CustomerId).Distinct().Count();
                }

                // Calculate previous period for comparison
                var previousPeriodDuration = FilterEndDate - FilterStartDate;
                var previousPeriodStart = FilterStartDate.AddDays(-previousPeriodDuration.TotalDays - 1);
                var previousPeriodEnd = FilterStartDate.AddDays(-1);
                
                var previousPeriodOrders = orders
                    .Where(o => o.OrderDate >= previousPeriodStart && o.OrderDate <= previousPeriodEnd)
                    .ToList();

                // Calculate previous period metrics
                var previousRevenue = previousPeriodOrders.Sum(o => o.TotalAmount);
                var previousOrdersCount = previousPeriodOrders.Count;
                
                // Count previous period customers using the same logic as current period
                var previousCustomers = previousPeriodOrders
                    .Where(o => !string.IsNullOrEmpty(o.CustomerName))
                    .Select(o => o.CustomerName)
                    .Distinct()
                    .Count();
                
                // Fallback to customer ID if no names are available
                if (previousCustomers == 0)
                {
                    previousCustomers = previousPeriodOrders.Select(o => o.CustomerId).Distinct().Count();
                }

                // Calculate percentage changes
                RevenueChangePercent = previousRevenue > 0 
                    ? ((TodayRevenue - previousRevenue) / previousRevenue) * 100 
                    : 0;
                    
                OrdersChangePercent = previousOrdersCount > 0 
                    ? ((TodayOrders - previousOrdersCount) / (decimal)previousOrdersCount) * 100 
                    : 0;
                    
                CustomersChangePercent = previousCustomers > 0 
                    ? ((TodayCustomers - previousCustomers) / (decimal)previousCustomers) * 100 
                    : 0;

                // Get recent orders (last 5)
                RecentOrders = filteredOrders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToList();

                // Calculate weekly/period sales trend using filtered data
                WeeklySalesTrend = CalculatePeriodSalesTrend(filteredOrders);

                // Calculate food category distribution from filtered orders
                var foodItems = await _foodService.GetAllFoodAsync(1, 100);
                var categories = await _foodService.GetAllCategoriesAsync();
                
                // Calculate category distribution based on filtered ordered items
                var categoryDistribution = new Dictionary<string, int>();
                foreach (var category in categories)
                {
                    var categoryItems = foodItems.Where(f => f.FoodCategoryId == category.Id);
                    var orderCount = filteredOrders
                        .SelectMany(o => o.Items)
                        .Count(i => categoryItems.Any(f => f.Id == i.FoodId));
                    categoryDistribution[category.Name] = orderCount;
                }

                // Replace OrderTypeDistribution with filtered CategoryDistribution
                OrderTypeDistribution = categoryDistribution;

                // Calculate order status distribution for filtered orders
                OrderStatusDistribution = filteredOrders
                    .GroupBy(o => o.Status)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Calculate orders by time of day for filtered period
                OrdersByTimeOfDay = CalculateOrdersByTimeOfDay(filteredOrders);

                // Get popular items based on actual filtered order data
                var popularItemsWithCount = filteredOrders
                    .SelectMany(o => o.Items)
                    .GroupBy(i => i.FoodId)
                    .Select(g => new 
                    { 
                        FoodId = g.Key, 
                        Count = g.Sum(i => i.Quantity),
                        Name = g.First().FoodName
                    })
                    .OrderByDescending(g => g.Count)
                    .Take(5)
                    .ToList();

                // Get popular items based on filtered orders, or fallback to default items if none exist
                if (popularItemsWithCount.Any())
                {
                    MostOrderedItems = foodItems
                        .Where(f => popularItemsWithCount.Select(p => p.FoodId).Contains(f.Id))
                        .ToList();
                        
                    // Set the order count for each item
                    foreach (var item in MostOrderedItems)
                    {
                        var count = popularItemsWithCount.FirstOrDefault(p => p.FoodId == item.Id)?.Count ?? 0;
                        item.OrderCount = count;
                    }
                }
                else
                {
                    // Provide fallback default items if no order data exists
                    MostOrderedItems = foodItems
                        .OrderBy(f => f.Name)  // Just use alphabetical order as fallback
                        .Take(5)
                        .ToList();
                        
                    // Set default order counts
                    foreach (var item in MostOrderedItems)
                    {
                        item.OrderCount = 0;
                    }
                }
                
                // Ensure we have descriptions and other properties populated to prevent UI flicker
                foreach (var item in MostOrderedItems)
                {
                    item.Description = item.Description ?? "Delicious menu item";
                    item.ImageUrl = !string.IsNullOrEmpty(item.ImageUrl) ? item.ImageUrl : "/images/placeholder-food.jpg";
                    item.FoodCategoryName = item.FoodCategoryName ?? "Uncategorized";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
            }

            return Page();
        }

        // Method to set date range based on filter
        private void CalculateDateRange()
        {
            var today = DateTime.Today;
            FilterEndDate = today.AddDays(1).AddSeconds(-1); // End of today

            switch (DateFilter)
            {
                case DateFilterType.Today:
                    FilterStartDate = today;
                    break;
                case DateFilterType.Week:
                    // Last 7 days including today
                    FilterStartDate = today.AddDays(-6);
                    break;
                case DateFilterType.Month:
                    // Last 30 days including today
                    FilterStartDate = today.AddDays(-29);
                    break;
                case DateFilterType.Year:
                    // Last 365 days including today
                    FilterStartDate = today.AddDays(-364);
                    break;
                case DateFilterType.AllTime:
                    // Set to a far past date to include all orders
                    FilterStartDate = new DateTime(2000, 1, 1);
                    break;
                default:
                    FilterStartDate = today;
                    break;
            }
        }

        // Method to calculate sales trend for the selected period
        private Dictionary<string, decimal> CalculatePeriodSalesTrend(List<Models.OrderModel> orders)
        {
            var result = new Dictionary<string, decimal>();
            
            // If the period is Week, show daily data
            if (DateFilter == DateFilterType.Week)
            {
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
                    var date = DateTime.Today.AddDays(-i);
                    var dayName = daysOfWeek[(int)date.DayOfWeek > 0 ? (int)date.DayOfWeek - 1 : 6]; // Adjust for Sunday
                    
                    var dayRevenue = orders
                        .Where(o => o.OrderDate.Date == date)
                        .Sum(o => o.TotalAmount);
                        
                    result[dayName] = dayRevenue;
                }
            }
            // If the period is Month, show weekly data
            else if (DateFilter == DateFilterType.Month)
            {
                for (int i = 0; i < 4; i++)
                {
                    var weekStart = DateTime.Today.AddDays(-i * 7 - 6);
                    var weekEnd = DateTime.Today.AddDays(-i * 7);
                    var weekLabel = $"Week {4-i}";
                    
                    var weekRevenue = orders
                        .Where(o => o.OrderDate.Date >= weekStart && o.OrderDate.Date <= weekEnd)
                        .Sum(o => o.TotalAmount);
                        
                    result[weekLabel] = weekRevenue;
                }
            }
            // If the period is Year, show monthly data
            else if (DateFilter == DateFilterType.Year)
            {
                var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                var today = DateTime.Today;
                
                for (int i = 11; i >= 0; i--)
                {
                    var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    var monthLabel = monthNames[monthStart.Month - 1];
                    
                    var monthRevenue = orders
                        .Where(o => o.OrderDate.Date >= monthStart && o.OrderDate.Date <= monthEnd)
                        .Sum(o => o.TotalAmount);
                        
                    result[monthLabel] = monthRevenue;
                }
            }
            // If AllTime or Today, show appropriate data
            else
            {
                if (DateFilter == DateFilterType.Today)
                {
                    // For Today, show hourly data
                    for (int hour = 0; hour < 24; hour += 2)
                    {
                        var hourStart = DateTime.Today.AddHours(hour);
                        var hourEnd = DateTime.Today.AddHours(hour + 2);
                        var hourLabel = $"{hour:D2}-{(hour + 2) % 24:D2}";
                        
                        var hourRevenue = orders
                            .Where(o => o.OrderDate >= hourStart && o.OrderDate < hourEnd)
                            .Sum(o => o.TotalAmount);
                            
                        result[hourLabel] = hourRevenue;
                    }
                }
                else // AllTime
                {
                    // For AllTime, show monthly data for the last 12 months
                    var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                    var today = DateTime.Today;
                    
                    for (int i = 11; i >= 0; i--)
                    {
                        var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        var monthLabel = $"{monthNames[monthStart.Month - 1]} {monthStart.Year}";
                        
                        var monthRevenue = orders
                            .Where(o => o.OrderDate.Date >= monthStart && o.OrderDate.Date <= monthEnd)
                            .Sum(o => o.TotalAmount);
                            
                        result[monthLabel] = monthRevenue;
                    }
                }
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
            if (string.IsNullOrEmpty(status))
                return "pending";
                
            return status.ToLower() switch
            {
                "completed" => "completed",
                "delivered" => "completed",
                "accepted" => "preparing",
                "preparing" => "preparing",
                "in progress" => "preparing",
                "pending" => "pending",
                "cancelled" => "cancelled",
                _ => "pending" // Default case
            };
        }
    }
} 