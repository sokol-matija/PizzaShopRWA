using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    /// <summary>
    /// Interface for trip-related operations
    /// </summary>
    public interface ITripService
    {
        /// <summary>
        /// Get all available trips
        /// </summary>
        Task<List<TripModel>> GetAllTripsAsync();
        
        /// <summary>
        /// Get a specific trip by ID
        /// </summary>
        Task<TripModel?> GetTripByIdAsync(int id);
        
        /// <summary>
        /// Get all trips for a specific destination
        /// </summary>
        Task<List<TripModel>> GetTripsByDestinationAsync(int destinationId);
        
        /// <summary>
        /// Create a new trip (admin only)
        /// </summary>
        Task<TripModel?> CreateTripAsync(TripModel trip);
        
        /// <summary>
        /// Update an existing trip (admin only)
        /// </summary>
        Task<TripModel?> UpdateTripAsync(int id, TripModel trip);
        
        /// <summary>
        /// Delete a trip (admin only)
        /// </summary>
        Task<bool> DeleteTripAsync(int id);
        
        /// <summary>
        /// Assign a guide to a trip (admin only)
        /// </summary>
        Task<bool> AssignGuideToTripAsync(int tripId, int guideId);
        
        /// <summary>
        /// Remove a guide from a trip (admin only)
        /// </summary>
        Task<bool> RemoveGuideFromTripAsync(int tripId, int guideId);
    }
} 