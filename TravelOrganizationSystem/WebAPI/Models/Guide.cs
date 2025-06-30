using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class Guide
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Bio { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        [Phone]
        public string Phone { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public int? YearsOfExperience { get; set; }

        // Navigation property
        public ICollection<TripGuide> TripGuides { get; set; }
    }
} 