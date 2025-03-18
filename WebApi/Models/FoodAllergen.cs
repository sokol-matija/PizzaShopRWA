namespace WebAPI.Models
{
	public class FoodAllergen
	{
		public int FoodId { get; set; }
		public Food Food { get; set; }

		public int AllergenId { get; set; }
		public Allergen Allergen { get; set; }
	}
}