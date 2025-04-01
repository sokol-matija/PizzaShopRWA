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
        
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;
        
        public string? ImageUrl { get; set; }
        
        [Display(Name = "Climate")]
        [StringLength(200, ErrorMessage = "Climate description cannot exceed 200 characters")]
        public string? Climate { get; set; }
        
        [Display(Name = "Best Time to Visit")]
        [StringLength(200, ErrorMessage = "Best time to visit description cannot exceed 200 characters")]
        public string? BestTimeToVisit { get; set; }
        
        /// <summary>
        /// Nickname or famous tagline for the destination (e.g., "The City of Light" for Paris)
        /// </summary>
        [Display(Name = "Tagline")]
        [StringLength(200, ErrorMessage = "Tagline cannot exceed 200 characters")]
        public string? Tagline { get; set; }
        
        // Navigation property for related trips (optional)
        public List<TripModel>? Trips { get; set; }
        
        // Computed property to show full location
        public string Location => $"{City}, {Country}";
    }

    /// <summary>
    /// Model for creating a new destination
    /// </summary>
    public class CreateDestinationModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
        public string? Description { get; set; }
        
        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot be longer than 100 characters")]
        public string Country { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot be longer than 100 characters")]
        public string City { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Climate")]
        [StringLength(200, ErrorMessage = "Climate description cannot exceed 200 characters")]
        public string? Climate { get; set; }

        [Display(Name = "Best Time to Visit")]
        [StringLength(200, ErrorMessage = "Best time to visit description cannot exceed 200 characters")]
        public string? BestTimeToVisit { get; set; }
    }
} 