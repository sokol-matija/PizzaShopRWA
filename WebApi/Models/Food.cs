using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
	public class Food
	{
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(500)]
		public string Description { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
		public decimal Price { get; set; }

		[StringLength(500)]
		[Url]
		public string ImageUrl { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Preparation time must be greater than 0")]
		public int? PreparationTime { get; set; }

		[Required]
		public int FoodCategoryId { get; set; }

		// Navigation properties
		public FoodCategory FoodCategory { get; set; }
		public ICollection<FoodAllergen> FoodAllergens { get; set; }
		public ICollection<OrderItem> OrderItems { get; set; }
	}
}