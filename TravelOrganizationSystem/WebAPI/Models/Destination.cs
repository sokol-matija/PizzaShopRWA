using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class Destination
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Country { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        // Navigation property - this is needed for Entity Framework relationships
        // but won't be serialized in API responses
        public virtual ICollection<Trip>? Trips { get; set; }
    }
}