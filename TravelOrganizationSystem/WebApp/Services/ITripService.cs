using WebApp.Models;

namespace WebApp.Services
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
        /// Get trips with pagination support
        /// </summary>
        Task<(List<TripModel> trips, int totalCount)> GetTripsAsync(int page = 1, int pageSize = 10, int? destinationId = null);
        
        /// <summary>
        /// Search trips with pagination support
        /// </summary>
        Task<(List<TripModel> trips, int totalCount)> SearchTripsAsync(string? name, string? description, int page = 1, int pageSize = 10);
        
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

        /// <summary>
        /// Book a trip for the current user
        /// </summary>
        Task<bool> BookTripAsync(int tripId, int numberOfParticipants);

        /// <summary>
        /// Get all trips booked by the current user
        /// </summary>
        Task<List<TripRegistrationModel>> GetUserTripsAsync();

        /// <summary>
        /// Cancel a trip booking for the current user
        /// </summary>
        Task<bool> CancelBookingAsync(int bookingId);
    }
} 