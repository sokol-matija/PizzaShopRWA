using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Services
{
	public interface IOrderService
	{
		Task<List<OrderDTO>> GetUserOrdersAsync(int userId);
		Task<OrderDTO> GetOrderByIdAsync(int orderId, int userId);
		Task<OrderDTO> CreateOrderAsync(OrderCreateDTO orderDto, int userId);
		Task<OrderDTO> UpdateOrderStatusAsync(int orderId, string status);
		Task<bool> CancelOrderAsync(int orderId, int userId);
		Task<PagedResultDTO<OrderDTO>> GetAllOrdersAsync(OrderFilterDTO filterDto);
	}

	public class OrderService : IOrderService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogService _logService;
		private readonly IUserService _userService;

		public OrderService(ApplicationDbContext context, ILogService logService, IUserService userService)
		{
			_context = context;
			_logService = logService;
			_userService = userService;
		}

		public async Task<List<OrderDTO>> GetUserOrdersAsync(int userId)
		{
			var orders = await _context.Orders
				.Include(o => o.User)
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.Food)
				.Where(o => o.UserId == userId)
				.OrderByDescending(o => o.OrderDate)
				.ToListAsync();

			var orderDtos = orders.Select(o => MapToOrderDTO(o)).ToList();

			await _logService.LogInformationAsync($"Retrieved {orderDtos.Count} orders for user with id={userId}");
			return orderDtos;
		}

		public async Task<OrderDTO> GetOrderByIdAsync(int orderId, int userId)
		{
			var order = await _context.Orders
				.Include(o => o.User)
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.Food)
				.FirstOrDefaultAsync(o => o.Id == orderId && (o.UserId == userId || IsUserAdmin(userId)));

			if (order == null)
			{
				await _logService.LogWarningAsync($"Order with id={orderId} not found or not accessible by user with id={userId}");
				return null;
			}

			await _logService.LogInformationAsync($"Retrieved order with id={orderId} for user with id={userId}");
			return MapToOrderDTO(order);
		}

		public async Task<OrderDTO> CreateOrderAsync(OrderCreateDTO orderDto, int userId)
		{
			// Validate order items
			if (orderDto.Items == null || !orderDto.Items.Any())
			{
				await _logService.LogWarningAsync($"Cannot create order: no items specified");
				return null;
			}

			// Calculate total amount and validate food items
			decimal totalAmount = 0;
			var orderItems = new List<OrderItem>();

			foreach (var item in orderDto.Items)
			{
				var food = await _context.Foods.FindAsync(item.FoodId);
				if (food == null)
				{
					await _logService.LogWarningAsync($"Cannot create order: food with id={item.FoodId} not found");
					return null;
				}

				totalAmount += food.Price * item.Quantity;
				orderItems.Add(new OrderItem
				{
					FoodId = item.FoodId,
					Quantity = item.Quantity,
					Price = food.Price
				});
			}

			// Get user's address if not provided
			var user = await _userService.GetByIdAsync(userId);
			var deliveryAddress = !string.IsNullOrEmpty(orderDto.DeliveryAddress)
				? orderDto.DeliveryAddress
				: user.Address;

			if (string.IsNullOrEmpty(deliveryAddress))
			{
				await _logService.LogWarningAsync($"Cannot create order: no delivery address provided");
				return null;
			}

			// Create new order
			var order = new Order
			{
				UserId = userId,
				OrderDate = DateTime.Now,
				TotalAmount = totalAmount,
				DeliveryAddress = deliveryAddress,
				Status = "Pending",
				OrderItems = orderItems
			};

			_context.Orders.Add(order);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Order with id={order.Id} was created for user with id={userId}");

			// Load related data for the response
			await _context.Entry(order)
				.Reference(o => o.User)
				.LoadAsync();

			return MapToOrderDTO(order);
		}

		public async Task<OrderDTO> UpdateOrderStatusAsync(int orderId, string status)
		{
			// Find the order to update
			var order = await _context.Orders
				.Include(o => o.User)
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.Food)
				.FirstOrDefaultAsync(o => o.Id == orderId);

			if (order == null)
			{
				await _logService.LogWarningAsync($"Cannot update order status: order with id={orderId} not found");
				return null;
			}

			// Validate status
			var validStatuses = new[] { "Pending", "Accepted", "In Progress", "Delivered", "Cancelled" };
			if (!validStatuses.Contains(status))
			{
				await _logService.LogWarningAsync($"Cannot update order status: invalid status '{status}'");
				return null;
			}

			// Update status
			order.Status = status;
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Order with id={orderId} status updated to '{status}'");
			return MapToOrderDTO(order);
		}

		public async Task<bool> CancelOrderAsync(int orderId, int userId)
		{
			// Find the order to cancel
			var order = await _context.Orders
				.FirstOrDefaultAsync(o => o.Id == orderId && (o.UserId == userId || IsUserAdmin(userId)));

			if (order == null)
			{
				await _logService.LogWarningAsync($"Cannot cancel order: order with id={orderId} not found or not accessible by user with id={userId}");
				return false;
			}

			// Check if order can be cancelled
			if (order.Status == "Delivered")
			{
				await _logService.LogWarningAsync($"Cannot cancel order with id={orderId}: order has already been delivered");
				return false;
			}

			// Update status to cancelled
			order.Status = "Cancelled";
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Order with id={orderId} was cancelled by user with id={userId}");
			return true;
		}

		public async Task<PagedResultDTO<OrderDTO>> GetAllOrdersAsync(OrderFilterDTO filterDto)
		{
			// Ensure valid pagination parameters
			if (filterDto.Page < 1) filterDto.Page = 1;
			if (filterDto.Count < 1) filterDto.Count = 10;

			// Start with all orders
			var query = _context.Orders
				.Include(o => o.User)
				.Include(o => o.OrderItems)
				.ThenInclude(oi => oi.Food)
				.AsQueryable();

			// Apply filters if provided
			if (!string.IsNullOrEmpty(filterDto.Status))
			{
				query = query.Where(o => o.Status == filterDto.Status);
			}

			if (filterDto.FromDate.HasValue)
			{
				query = query.Where(o => o.OrderDate >= filterDto.FromDate.Value);
			}

			if (filterDto.ToDate.HasValue)
			{
				query = query.Where(o => o.OrderDate <= filterDto.ToDate.Value.AddDays(1));
			}

			if (filterDto.UserId.HasValue)
			{
				query = query.Where(o => o.UserId == filterDto.UserId.Value);
			}

			// Get total count after filtering
			var totalCount = await query.CountAsync();

			// Calculate total pages
			var pageCount = (int)Math.Ceiling(totalCount / (double)filterDto.Count);

			// Apply pagination
			var orders = await query
				.OrderByDescending(o => o.OrderDate)
				.Skip((filterDto.Page - 1) * filterDto.Count)
				.Take(filterDto.Count)
				.ToListAsync();

			// Map to DTOs
			var orderDtos = orders.Select(o => MapToOrderDTO(o)).ToList();

			await _logService.LogInformationAsync($"Retrieved {orderDtos.Count} orders with filters");

			// Return paged result
			return new PagedResultDTO<OrderDTO>
			{
				Items = orderDtos,
				TotalCount = totalCount,
				PageCount = pageCount,
				CurrentPage = filterDto.Page
			};
		}

		// Helper method to check if a user is an admin
		private bool IsUserAdmin(int userId)
		{
			var user = _context.Users.Find(userId);
			return user != null && user.IsAdmin;
		}

		// Helper method to map Order entity to OrderDTO
		private OrderDTO MapToOrderDTO(Order order)
		{
			return new OrderDTO
			{
				Id = order.Id,
				UserId = order.UserId,
				Username = order.User?.Username,
				OrderDate = order.OrderDate,
				TotalAmount = order.TotalAmount,
				DeliveryAddress = order.DeliveryAddress,
				Status = order.Status,
				Items = order.OrderItems?.Select(oi => new OrderItemDTO
				{
					Id = oi.Id,
					FoodId = oi.FoodId,
					FoodName = oi.Food?.Name,
					Quantity = oi.Quantity,
					Price = oi.Price
				}).ToList() ?? new List<OrderItemDTO>()
			};
		}
	}
}