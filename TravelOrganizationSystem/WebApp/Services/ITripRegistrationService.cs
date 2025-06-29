using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Interface for trip registration (booking) operations
    /// </summary>
    public interface ITripRegistrationService
    {
        /// <summary>
        /// Get all registrations (admin only)
        /// </summary>
        Task<List<TripRegistrationModel>> GetAllRegistrationsAsync();
        
        /// <summary>
        /// Get a specific registration by ID
        /// </summary>
        Task<TripRegistrationModel?> GetRegistrationByIdAsync(int id);
        
        /// <summary>
        /// Get all registrations for a specific user
        /// </summary>
        Task<List<TripRegistrationModel>> GetRegistrationsByUserAsync(int userId);
        
        /// <summary>
        /// Get all registrations for a specific trip (admin only)
        /// </summary>
        Task<List<TripRegistrationModel>> GetRegistrationsByTripAsync(int tripId);
        
        /// <summary>
        /// Create a new registration (book a trip)
        /// </summary>
        Task<TripRegistrationModel?> CreateRegistrationAsync(TripRegistrationModel registration);
        
        /// <summary>
        /// Update an existing registration
        /// </summary>
        Task<TripRegistrationModel?> UpdateRegistrationAsync(int id, TripRegistrationModel registration);
        
        /// <summary>
        /// Delete a registration (cancel a booking)
        /// </summary>
        Task<bool> DeleteRegistrationAsync(int id);
        
        /// <summary>
        /// Update the status of a registration (admin only)
        /// </summary>
        Task<bool> UpdateRegistrationStatusAsync(int id, string status);
    }
} 