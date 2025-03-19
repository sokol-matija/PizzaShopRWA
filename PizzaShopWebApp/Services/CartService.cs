using PizzaShopWebApp.Models;
using System.Text.Json;

namespace PizzaShopWebApp.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _cartKey = "CartItems";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<CartItemModel> GetCartItems()
        {
            var cartJson = _httpContextAccessor.HttpContext?.Session.GetString(_cartKey);
            if (string.IsNullOrEmpty(cartJson))
                return new List<CartItemModel>();

            return JsonSerializer.Deserialize<List<CartItemModel>>(cartJson) ?? new List<CartItemModel>();
        }

        public void AddItem(MenuItemModel item, int quantity, string? customizations = null)
        {
            var cart = GetCartItems();
            var existingItem = cart.FirstOrDefault(x => x.Id == item.Id);

            if (existingItem != null)
            {
                // If item exists with same customizations, increment quantity
                if (existingItem.Customizations == customizations)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    // Otherwise add as new item
                    cart.Add(new CartItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price,
                        ImageUrl = item.ImageUrl,
                        Quantity = quantity,
                        Customizations = customizations
                    });
                }
            }
            else
            {
                // Add new item
                cart.Add(new CartItemModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    ImageUrl = item.ImageUrl,
                    Quantity = quantity,
                    Customizations = customizations
                });
            }

            SaveCart(cart);
        }

        public void UpdateItemQuantity(int itemId, int quantity)
        {
            var cart = GetCartItems();
            var item = cart.FirstOrDefault(x => x.Id == itemId);
            
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    cart.Remove(item);
                }
                
                SaveCart(cart);
            }
        }

        public void RemoveItem(int itemId)
        {
            var cart = GetCartItems();
            cart.RemoveAll(x => x.Id == itemId);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(_cartKey);
        }

        public int GetTotalItems()
        {
            return GetCartItems().Sum(item => item.Quantity);
        }

        public decimal GetSubtotal()
        {
            return GetCartItems().Sum(item => item.Price * item.Quantity);
        }

        private void SaveCart(List<CartItemModel> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            _httpContextAccessor.HttpContext?.Session.SetString(_cartKey, cartJson);
        }
    }
} 