using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    /// <summary>
    /// Represents a trip registration/booking
    /// </summary>
    public class TripRegistrationModel
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public int TripId { get; set; }
        
        public string TripName { get; set; } = string.Empty;
        
        public string DestinationName { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        
        public DateTime EndDate { get; set; }
        
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        [Required]
        [Range(1, 10, ErrorMessage = "Number of participants must be between 1 and 10")]
        [Display(Name = "Number of Participants")]
        public int NumberOfParticipants { get; set; } = 1;
        
        [Required]
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }
        
        [Required]
        public string Status { get; set; } = "Pending";
        
        // Navigation properties (may be null during serialization)
        public UserModel? User { get; set; }
        public TripModel? Trip { get; set; }
        
        // Computed properties
        public bool IsCancellable => Status == "Pending" || Status == "Confirmed";
        
        public string StatusBadgeClass => Status switch
        {
            "Confirmed" => "bg-success",
            "Pending" => "bg-warning",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
    }
} 