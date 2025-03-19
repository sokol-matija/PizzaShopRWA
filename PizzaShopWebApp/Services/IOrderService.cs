using PizzaShopWebApp.Models;

namespace PizzaShopWebApp.Services
{
    public interface IOrderService : IApiService
    {
        Task<IEnumerable<OrderModel>> GetUserOrdersAsync();
        Task<OrderModel> GetOrderByIdAsync(int id);
        Task<OrderModel> CreateOrderAsync(OrderCreateModel order);
        Task<bool> CancelOrderAsync(int id);
        Task<IEnumerable<OrderModel>> GetAllOrdersAsync(int page = 1, int count = 10);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
    }
} 