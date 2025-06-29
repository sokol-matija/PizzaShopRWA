using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    // DTO for GET operations - used when returning trip data
    public class TripDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public int DestinationId { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public List<GuideDTO> Guides { get; set; } = new List<GuideDTO>();
        public int AvailableSpots { get; set; }
    }

    // DTO for POST operations - used when creating a new trip
    public class CreateTripDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(500)]
        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "MaxParticipants must be greater than 0")]
        public int MaxParticipants { get; set; }

        [Required]
        public int DestinationId { get; set; }

        public List<int> GuideIds { get; set; } = new List<int>();
    }

    // DTO for PUT operations - used when updating a trip
    public class UpdateTripDTO
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(500)]
        [Url]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "MaxParticipants must be greater than 0")]
        public int MaxParticipants { get; set; }

        [Required]
        public int DestinationId { get; set; }
    }
} 