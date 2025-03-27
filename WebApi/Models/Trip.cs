using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class Trip
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "MaxParticipants must be greater than 0")]
        public int MaxParticipants { get; set; }

        [Required]
        public int DestinationId { get; set; }

        // Navigation properties
        public Destination Destination { get; set; }
        public ICollection<TripGuide> TripGuides { get; set; }
        public ICollection<TripRegistration> TripRegistrations { get; set; }
    }
} 