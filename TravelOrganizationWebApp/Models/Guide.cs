using System.ComponentModel.DataAnnotations;

namespace TravelOrganizationWebApp.Models
{
    public class Guide
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Bio cannot be longer than 500 characters")]
        public string Bio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Years of Experience")]
        [Range(0, 100, ErrorMessage = "Years of experience must be between 0 and 100")]
        public int? YearsOfExperience { get; set; }

        // Computed property for display
        public string DisplayName => $"{Name} ({YearsOfExperience ?? 0} years)";
    }
} 