using System.ComponentModel.DataAnnotations;

namespace TravelOrganizationWebApp.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters")]
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
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Maximum participants is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum participants must be greater than 0")]
        [Display(Name = "Maximum Participants")]
        public int MaxParticipants { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public int DestinationId { get; set; }
        
        // Additional properties for display
        public string DestinationName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public List<Guide> Guides { get; set; } = new List<Guide>();
        public int AvailableSpots { get; set; }
        
        // Computed properties
        public string Duration => (EndDate - StartDate).Days + 1 + " days";
        public bool IsSoldOut => AvailableSpots <= 0;
        public bool IsUpcoming => StartDate > DateTime.Today;
    }
} 