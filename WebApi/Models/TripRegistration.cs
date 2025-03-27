using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class TripRegistration
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        public DateTime RegistrationDate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Number of participants must be greater than 0")]
        public int NumberOfParticipants { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than 0")]
        public decimal TotalPrice { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Confirmed";

        // Navigation properties
        public User User { get; set; }
        public Trip Trip { get; set; }
    }
} 