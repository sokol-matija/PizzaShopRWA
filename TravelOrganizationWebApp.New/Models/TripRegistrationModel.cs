using System.ComponentModel.DataAnnotations;

namespace TravelOrganizationWebApp.Models
{
    /// <summary>
    /// Represents a trip registration (booking) in the system
    /// </summary>
    public class TripRegistrationModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Trip ID is required")]
        public int TripId { get; set; }
        
        [Required(ErrorMessage = "Registration date is required")]
        [Display(Name = "Registration Date")]
        [DataType(DataType.Date)]
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        
        [Required(ErrorMessage = "Number of participants is required")]
        [Display(Name = "Participants")]
        [Range(1, 20, ErrorMessage = "Number of participants must be between 1 and 20")]
        public int Participants { get; set; } = 1;
        
        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }
        
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = "Pending";
        
        [Display(Name = "Payment Status")]
        public string? PaymentStatus { get; set; } = "Unpaid";
        
        // Navigation properties for related data (optional in frontend model)
        public UserModel? User { get; set; }
        
        public TripModel? Trip { get; set; }
        
        // Computed properties
        public bool IsCancellable => Status == "Pending" || Status == "Confirmed";
        
        public bool IsModifiable => Status == "Pending" || Status == "Confirmed";
        
        public decimal? TotalPrice => Trip?.Price * Participants;
        
        public string FormattedTotalPrice => TotalPrice?.ToString("C") ?? "N/A";
    }
} 