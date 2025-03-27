using System.ComponentModel.DataAnnotations;

namespace TravelOrganizationWebApp.Models
{
    public class TripRegistration
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

        [Required(ErrorMessage = "Number of participants is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of participants must be greater than 0")]
        [Display(Name = "Number of Participants")]
        public int NumberOfParticipants { get; set; } = 1;

        [Display(Name = "Total Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status cannot be longer than 20 characters")]
        public string Status { get; set; } = "Pending";

        // Computed properties
        public bool IsConfirmed => Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase);
        public bool IsCancelled => Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);
        public bool IsPending => Status.Equals("Pending", StringComparison.OrdinalIgnoreCase);
        public string Duration => (EndDate - StartDate).Days + 1 + " days";
        public bool IsUpcoming => StartDate > DateTime.Today;
        public bool IsPast => EndDate < DateTime.Today;
        public bool IsActive => StartDate <= DateTime.Today && EndDate >= DateTime.Today;
    }
} 