using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    // DTO for GET operations - used when returning trip registration data
    public class TripRegistrationDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int TripId { get; set; }
        public string TripName { get; set; } = string.Empty;
        public string DestinationName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int NumberOfParticipants { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // DTO for POST operations - used when creating a new trip registration
    public class CreateTripRegistrationDTO
    {
        [Required]
        public int TripId { get; set; }

        public int? UserId { get; set; } // Optional: this will be set by the server for non-admin users

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of participants must be greater than 0")]
        public int NumberOfParticipants { get; set; } = 1;

        // Total price will be calculated on the server
    }

    // DTO for PUT operations - used when updating a trip registration
    public class UpdateTripRegistrationDTO
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Number of participants must be greater than 0")]
        public int NumberOfParticipants { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
    }

    // DTO for PATCH operations - used when updating only the status of a trip registration
    public class UpdateTripRegistrationStatusDTO
    {
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;
    }
} 