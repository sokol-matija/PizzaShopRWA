using System.ComponentModel.DataAnnotations;

namespace TravelOrganizationWebApp.Models
{
    /// <summary>
    /// Represents a tour guide in the system
    /// </summary>
    public class GuideModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        public string LastName { get; set; } = string.Empty;
        
        [Display(Name = "Biography")]
        public string? Bio { get; set; }
        
        [Display(Name = "Photo URL")]
        public string? PhotoUrl { get; set; }
        
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
        
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Languages")]
        public string? Languages { get; set; }
        
        [Display(Name = "Years of Experience")]
        [Range(0, 100, ErrorMessage = "Years of experience must be between 0 and 100")]
        public int? YearsExperience { get; set; }
        
        // Computed property for full name
        public string FullName => $"{FirstName} {LastName}";
    }
} 