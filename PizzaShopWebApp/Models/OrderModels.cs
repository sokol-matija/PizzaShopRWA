namespace PizzaShopWebApp.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<OrderItemModel> Items { get; set; } = new List<OrderItemModel>();
    }

    public class OrderItemModel
    {
        public int Id { get; set; }
        public int FoodId { get; set; }
        public string FoodName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class OrderCreateModel
    {
        public string DeliveryAddress { get; set; } = string.Empty;
        public List<OrderItemCreateModel> Items { get; set; } = new List<OrderItemCreateModel>();
    }

    public class OrderItemCreateModel
    {
        public int FoodId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Customizations { get; set; }
    }
} 