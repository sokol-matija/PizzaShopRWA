using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
	public class FoodCategoryDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class FoodCategoryCreateDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(500)]
		public string Description { get; set; }
	}

	public class FoodCategoryUpdateDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(500)]
		public string Description { get; set; }
	}

	public class AllergenDTO
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class AllergenCreateDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(500)]
		public string Description { get; set; }
	}

	public class AllergenUpdateDTO
	{
		[Required]
		[StringLength(100)]
		public string Name { get; set; }

		[StringLength(500)]
		public string Description { get; set; }
	}
}