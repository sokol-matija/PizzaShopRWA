using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    /// <summary>
    /// Represents a user in the system
    /// </summary>
    public class UserModel
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }
        
        [StringLength(200)]
        public string? Address { get; set; }
        
        public bool IsAdmin { get; set; }
        
        // Computed properties
        public string FullName => string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName) 
            ? Username 
            : $"{FirstName} {LastName}".Trim();
    }
} 