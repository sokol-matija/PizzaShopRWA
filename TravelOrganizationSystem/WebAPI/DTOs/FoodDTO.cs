using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
	public class FoodDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public string ImageUrl { get; set; }
		public int? PreparationTime { get; set; }
		public int FoodCategoryId { get; set; }
		public string FoodCategoryName { get; set; }
		public List<AllergenDTO> Allergens { get; set; }
	}

	public class FoodCreateDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(500)]
		public string Description { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
		public decimal Price { get; set; }

		[StringLength(500)]
		public string ImageUrl { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Preparation time must be greater than 0")]
		public int? PreparationTime { get; set; }

		[Required]
		public int FoodCategoryId { get; set; }

		public List<int> AllergenIds { get; set; } = new List<int>();
	}

	public class FoodUpdateDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(500)]
		public string Description { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
		public decimal Price { get; set; }

		[StringLength(500)]
		public string ImageUrl { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Preparation time must be greater than 0")]
		public int? PreparationTime { get; set; }

		[Required]	
		public int FoodCategoryId { get; set; }

		public List<int> AllergenIds { get; set; } = new List<int>();
	}

	public class FoodSearchDTO
	{
		public string? Name { get; set; }  // Make nullable (C# 8.0+)
		public string? Description { get; set; }  // Make nullable
		public int? CategoryId { get; set; }

		[Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
		public int Page { get; set; } = 1;

		[Range(1, int.MaxValue, ErrorMessage = "Count must be greater than 0")]
		public int Count { get; set; } = 10;
	}

	public class PagedResultDTO<T>
	{
		public List<T> Items { get; set; }
		public int TotalCount { get; set; }
		public int PageCount { get; set; }
		public int CurrentPage { get; set; }
	}
}