using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
	public class Order
	{
		public int Id { get; set; }

		[Required]
		public int UserId { get; set; }

		[Required]
		public DateTime OrderDate { get; set; } = DateTime.Now;

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
		public decimal TotalAmount { get; set; }

		[Required]
		[StringLength(200)]
		public string DeliveryAddress { get; set; }

		[Required]
		[StringLength(50)]
		public string Status { get; set; }

		// Navigation properties
		public User User { get; set; }
		public ICollection<OrderItem> OrderItems { get; set; }
	}
}