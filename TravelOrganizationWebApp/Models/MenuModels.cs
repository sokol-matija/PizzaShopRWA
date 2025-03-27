namespace TravelOrganizationWebApp.Models
{
    // Keeping this model but marking it as obsolete, as some existing code might reference it
    [System.Obsolete("Use appropriate travel models instead")]
    public class MenuItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int? PreparationTime { get; set; }
        public int FoodCategoryId { get; set; }
        public string FoodCategoryName { get; set; } = string.Empty;
        public List<AllergenModel> Allergens { get; set; } = new List<AllergenModel>();
        public bool IsVegetarian { get; set; }
        public bool IsPopular { get; set; }
        public int OrderCount { get; set; }
    }

    // Keeping these models as they might be useful for categorizing destinations or trips
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AllergenModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // Generic paged result model that will be useful for any paginated API response
    public class PagedResultModel<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
    }
} 