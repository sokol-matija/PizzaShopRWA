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
    public class OrderPageModel : PageModel
    {
        private readonly IFoodService _foodService;
        private readonly ICartService _cartService;
        private readonly IUserService _userService; 
        private readonly ILogger<OrderPageModel> _logger;

        public OrderPageModel(
            IFoodService foodService,
            ICartService cartService,
            IUserService userService,
            ILogger<OrderPageModel> logger)
        {
            _foodService = foodService;
            _cartService = cartService;
            _userService = userService;
            _logger = logger;
        }

        public List<MenuItemModel> Foods { get; set; } = new List<MenuItemModel>();
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public List<CartItemModel> CartItems { get; set; } = new List<CartItemModel>();
        
        public string SearchTerm { get; set; } = string.Empty;
        public int? SelectedCategoryId { get; set; }

        public async Task<IActionResult> OnGetAsync(string searchTerm = "", int? categoryId = null)
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            SearchTerm = searchTerm;
            SelectedCategoryId = categoryId;

            try
            {
                // Load categories
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                
                // Load foods based on search and category filters
                if (!string.IsNullOrEmpty(searchTerm) || categoryId.HasValue)
                {
                    Foods = (await _foodService.SearchFoodAsync(searchTerm, categoryId)).ToList();
                }
                else
                {
                    Foods = (await _foodService.GetAllFoodAsync()).ToList();
                }

                // Get cart items
                CartItems = _cartService.GetCartItems();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading menu data for order page");
                // We could show an error message here if desired
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddToCartAsync(int foodId, int quantity = 1)
        {
            if (quantity <= 0)
                quantity = 1;

            try
            {
                var food = await _foodService.GetFoodByIdAsync(foodId);
                if (food != null && food.Id > 0)
                {
                    _cartService.AddItem(food, quantity);
                    TempData["SuccessMessage"] = $"{food.Name} added to cart.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Food item not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                TempData["ErrorMessage"] = "An error occurred while adding to cart.";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostRemoveFromCartAsync(int itemId)
        {
            try
            {
                _cartService.RemoveItem(itemId);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                TempData["ErrorMessage"] = "An error occurred while removing from cart.";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostUpdateCartAsync(int itemId, int quantity)
        {
            try
            {
                _cartService.UpdateItemQuantity(itemId, quantity);
                TempData["SuccessMessage"] = "Cart updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart");
                TempData["ErrorMessage"] = "An error occurred while updating cart.";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostClearCartAsync()
        {
            try
            {
                _cartService.ClearCart();
                TempData["SuccessMessage"] = "Cart cleared.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                TempData["ErrorMessage"] = "An error occurred while clearing cart.";
            }

            return RedirectToPage();
        }

        public decimal GetCartTotal()
        {
            return CartItems.Sum(item => item.Price * item.Quantity);
        }

        public int GetTotalItems()
        {
            return CartItems.Sum(item => item.Quantity);
        }
    }
} 