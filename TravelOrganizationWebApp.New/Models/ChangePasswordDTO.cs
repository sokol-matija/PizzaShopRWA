namespace TravelOrganizationWebApp.Models
{
    /// <summary>
    /// Data transfer object for password change requests to the API
    /// </summary>
    public class ChangePasswordDTO
    {
        /// <summary>
        /// The user's current password
        /// </summary>
        public string CurrentPassword { get; set; } = string.Empty;
        
        /// <summary>
        /// The new password to set
        /// </summary>
        public string NewPassword { get; set; } = string.Empty;
        
        /// <summary>
        /// Confirmation of the new password
        /// </summary>
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
} 