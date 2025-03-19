namespace PizzaShopWebApp.Models
{
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
    }

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

    public class PagedResultModel<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageCount { get; set; }
        public int CurrentPage { get; set; }
    }
} 