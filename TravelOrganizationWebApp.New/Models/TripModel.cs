using System.ComponentModel.DataAnnotations;

namespace TravelOrganizationWebApp.Models
{
    /// <summary>
    /// Represents a travel trip in the system
    /// </summary>
    public class TripModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }
        
        [Display(Name = "Current Bookings")]
        public int CurrentBookings { get; set; }
        
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }
        
        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public int DestinationId { get; set; }
        
        public string? DestinationName { get; set; }
        
        // Navigation property for guides (optional in frontend model)
        public List<GuideModel>? Guides { get; set; }
        
        // Computed properties
        public int AvailableSlots => Capacity - CurrentBookings;
        
        public bool IsFull => CurrentBookings >= Capacity;
        
        public int DurationInDays => (EndDate - StartDate).Days + 1;
        
        // Format price in USD regardless of system culture
        public string FormattedPrice => $"${Price:0.00}";
    }
} 