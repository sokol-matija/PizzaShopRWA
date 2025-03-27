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
    public class PaymentModel : PageModel
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ILogger<PaymentModel> _logger;

        public PaymentModel(
            ICartService cartService,
            IOrderService orderService,
            IUserService userService,
            ILogger<PaymentModel> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userService = userService;
            _logger = logger;
        }

        [BindProperty]
        public string DeliveryAddress { get; set; } = string.Empty;

        [BindProperty]
        public string PaymentMethod { get; set; } = "Credit Card";

        public List<CartItemModel> CartItems { get; set; } = new List<CartItemModel>();
        
        public decimal Subtotal => CartItems.Sum(item => item.Price * item.Quantity);
        public decimal Tax => Subtotal * 0.08m; // 8% tax
        public decimal DeliveryFee => 3.99m;
        public decimal Total => Subtotal + Tax + DeliveryFee;

        public IActionResult OnGet()
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            // Get cart items
            CartItems = _cartService.GetCartItems();
            
            // If cart is empty, redirect to order page
            if (!CartItems.Any())
            {
                return RedirectToPage("Order");
            }

            // Get user profile for default address
            var userProfile = _userService.GetUserProfileAsync();
            DeliveryAddress = userProfile.Address ?? string.Empty;

            return Page();
        }

        public async Task<IActionResult> OnPostCheckoutAsync()
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            // Get cart items
            CartItems = _cartService.GetCartItems();
            
            // If cart is empty, redirect to order page
            if (!CartItems.Any())
            {
                return RedirectToPage("Order");
            }

            // Validate delivery address
            if (string.IsNullOrWhiteSpace(DeliveryAddress))
            {
                ModelState.AddModelError("DeliveryAddress", "Delivery address is required");
                return Page();
            }

            try
            {
                // Create order
                var orderItems = CartItems.Select(item => new OrderItemCreateModel
                {
                    FoodId = item.Id,
                    Quantity = item.Quantity
                }).ToList();

                var orderCreate = new OrderCreateModel
                {
                    DeliveryAddress = DeliveryAddress,
                    Items = orderItems
                };

                var result = await _orderService.CreateOrderAsync(orderCreate);

                if (result != null && result.Id > 0)
                {
                    // Clear cart after successful order
                    _cartService.ClearCart();
                    
                    // Redirect to order confirmation page or dashboard
                    TempData["SuccessMessage"] = $"Order placed successfully. Your order number is {result.OrderNumber}.";
                    return RedirectToPage("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create order. Please try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order during checkout");
                ModelState.AddModelError("", "An error occurred during checkout. Please try again.");
                return Page();
            }
        }

        public IActionResult OnPostCancelAsync()
        {
            return RedirectToPage("Order");
        }
    }
} 