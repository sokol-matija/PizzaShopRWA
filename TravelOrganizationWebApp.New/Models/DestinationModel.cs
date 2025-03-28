using System.ComponentModel.DataAnnotations;

namespace TravelOrganizationWebApp.Models
{
    /// <summary>
    /// Represents a travel destination in the system
    /// </summary>
    public class DestinationModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        
        public string? ImageUrl { get; set; }
        
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }
        
        [Required(ErrorMessage = "Max participants is required")]
        [Range(1, 100, ErrorMessage = "Max participants must be between 1 and 100")]
        public int MaxParticipants { get; set; }
        
        [Required(ErrorMessage = "Current participants is required")]
        [Range(0, 100, ErrorMessage = "Current participants must be between 0 and 100")]
        public int CurrentParticipants { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Climate")]
        [StringLength(200, ErrorMessage = "Climate description cannot exceed 200 characters")]
        public string? Climate { get; set; }
        
        [Display(Name = "Best Time to Visit")]
        [StringLength(200, ErrorMessage = "Best time to visit description cannot exceed 200 characters")]
        public string? BestTimeToVisit { get; set; }
        
        // Computed property to show full location
        public string Location => $"{Name}, {Country}";
    }

    /// <summary>
    /// Model for creating a new destination
    /// </summary>
    public class CreateDestinationModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot be longer than 100 characters")]
        public string Country { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Climate")]
        [StringLength(200, ErrorMessage = "Climate description cannot exceed 200 characters")]
        public string? Climate { get; set; }

        [Display(Name = "Best Time to Visit")]
        [StringLength(200, ErrorMessage = "Best time to visit description cannot exceed 200 characters")]
        public string? BestTimeToVisit { get; set; }
    }
} 