using System.ComponentModel.DataAnnotations;

namespace TravelOrganizationWebApp.Models
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class UserModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;
        
        public string? FirstName { get; set; }
        
        public string? LastName { get; set; }
        
        public string? PhoneNumber { get; set; }
        
        public string? Address { get; set; }
        
        public bool IsAdmin { get; set; }
        
        // Additional properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
    }
} 