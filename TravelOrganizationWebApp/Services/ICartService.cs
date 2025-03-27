using PizzaShopWebApp.Models;

namespace PizzaShopWebApp.Services
{
    public interface ICartService
    {
        List<CartItemModel> GetCartItems();
        void AddItem(MenuItemModel item, int quantity, string? customizations = null);
        void UpdateItemQuantity(int itemId, int quantity);
        void RemoveItem(int itemId);
        void ClearCart();
        int GetTotalItems();
        decimal GetSubtotal();
    }
} 