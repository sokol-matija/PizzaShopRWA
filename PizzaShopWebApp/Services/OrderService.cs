using System.Net.Http.Json;
using PizzaShopWebApp.Models;

namespace PizzaShopWebApp.Services
{
    public class OrderService : ApiServiceBase, IOrderService
    {
        public OrderService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<OrderService> logger) 
            : base(httpClientFactory, httpContextAccessor, configuration, logger)
        {
        }

        public async Task<IEnumerable<OrderModel>> GetUserOrdersAsync()
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.GetAsync("/api/order");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<OrderModel>>();
                    return result ?? new List<OrderModel>();
                }
                
                _logger.LogWarning("Failed to get user orders. Status code: {StatusCode}", response.StatusCode);
                return new List<OrderModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user orders");
                return new List<OrderModel>();
            }
        }

        public async Task<OrderModel> GetOrderByIdAsync(int id)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.GetAsync($"/api/order/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<OrderModel>();
                    return result ?? new OrderModel();
                }
                
                _logger.LogWarning("Failed to get order by id. Status code: {StatusCode}", response.StatusCode);
                return new OrderModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by id");
                return new OrderModel();
            }
        }

        public async Task<OrderModel> CreateOrderAsync(OrderCreateModel order)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.PostAsJsonAsync("/api/order", order);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<OrderModel>();
                    return result ?? new OrderModel();
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create order. Status code: {StatusCode}, Error: {Error}", 
                    response.StatusCode, errorContent);
                    
                return new OrderModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return new OrderModel();
            }
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.DeleteAsync($"/api/order/{id}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order");
                return false;
            }
        }

        public async Task<IEnumerable<OrderModel>> GetAllOrdersAsync(int page = 1, int count = 10)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.GetAsync($"/api/order/all?page={page}&count={count}&includeUserDetails=true");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PagedResultModel<OrderModel>>();
                    var orders = result?.Items ?? new List<OrderModel>();
                    
                    foreach (var order in orders.Where(o => string.IsNullOrEmpty(o.CustomerName)))
                    {
                        if (string.IsNullOrEmpty(order.CustomerName))
                        {
                            order.CustomerName = $"Customer #{order.CustomerId}";
                        }
                    }
                    
                    return orders;
                }
                
                _logger.LogWarning("Failed to get all orders. Status code: {StatusCode}", response.StatusCode);
                return new List<OrderModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all orders");
                return new List<OrderModel>();
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            try
            {
                var client = await GetHttpClientAsync();
                var response = await client.PutAsJsonAsync($"/api/order/{id}/status", new { Status = status });
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status");
                return false;
            }
        }
    }
} 