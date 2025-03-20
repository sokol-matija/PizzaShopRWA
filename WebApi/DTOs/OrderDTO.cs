using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
	public class OrderDTO
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string Username { get; set; }
		public string CustomerName { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal TotalAmount { get; set; }
		public string DeliveryAddress { get; set; }
		public string Status { get; set; }
		public List<OrderItemDTO> Items { get; set; }
	}

	public class OrderItemDTO
	{
		public int Id { get; set; }
		public int FoodId { get; set; }
		public string FoodName { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
	}

	public class OrderCreateDTO
	{
		[Required]
		[StringLength(200)]
		public string DeliveryAddress { get; set; }

		[Required]
		public List<OrderItemCreateDTO> Items { get; set; }
	}

	public class OrderItemCreateDTO
	{
		[Required]
		public int FoodId { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
		public int Quantity { get; set; }
	}

	public class OrderStatusUpdateDTO
	{
		[Required]
		[StringLength(50)]
		public string Status { get; set; }
	}

	public class OrderFilterDTO
	{
		public string Status { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public int? UserId { get; set; }
		public int Page { get; set; } = 1;
		public int Count { get; set; } = 10;
		public bool IncludeUserDetails { get; set; } = false;
	}
}