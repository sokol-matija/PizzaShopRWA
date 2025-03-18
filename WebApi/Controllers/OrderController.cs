using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize] // All order endpoints require authentication
	public class OrderController : ControllerBase
	{
		private readonly IOrderService _orderService;
		private readonly ILogService _logService;

		public OrderController(IOrderService orderService, ILogService logService)
		{
			_orderService = orderService;
			_logService = logService;
		}

		// GET: api/order
		[HttpGet]
		public async Task<IActionResult> GetUserOrders()
		{
			// Get user ID from token claims
			if (!int.TryParse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value, out int userId))
				return Unauthorized();

			var orders = await _orderService.GetUserOrdersAsync(userId);
			return Ok(orders);
		}

		// GET: api/order/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			// Get user ID from token claims
			if (!int.TryParse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value, out int userId))
				return Unauthorized();

			var order = await _orderService.GetOrderByIdAsync(id, userId);
			if (order == null)
				return NotFound();

			return Ok(order);
		}

		// POST: api/order
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] OrderCreateDTO orderDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Get user ID from token claims
			if (!int.TryParse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value, out int userId))
				return Unauthorized();

			var createdOrder = await _orderService.CreateOrderAsync(orderDto, userId);
			if (createdOrder == null)
				return BadRequest("Failed to create order. Please check that all food items exist.");

			return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
		}

		// PUT: api/order/5/status
		[HttpPut("{id}/status")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDTO statusDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, statusDto.Status);
			if (updatedOrder == null)
				return NotFound();

			return Ok(updatedOrder);
		}

		// DELETE: api/order/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> CancelOrder(int id)
		{
			// Get user ID from token claims
			if (!int.TryParse(User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value, out int userId))
				return Unauthorized();

			var result = await _orderService.CancelOrderAsync(id, userId);
			if (!result)
				return NotFound();

			return NoContent();
		}

		// GET: api/order/all
		[HttpGet("all")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> GetAllOrders(
			[FromQuery] string status = null,
			[FromQuery] DateTime? fromDate = null,
			[FromQuery] DateTime? toDate = null,
			[FromQuery] int? userId = null,
			[FromQuery] int page = 1,
			[FromQuery] int count = 10)
		{
			var filterDto = new OrderFilterDTO
			{
				Status = status,
				FromDate = fromDate,
				ToDate = toDate,
				UserId = userId,
				Page = page,
				Count = count
			};

			var orders = await _orderService.GetAllOrdersAsync(filterDto);
			return Ok(orders);
		}
	}
}