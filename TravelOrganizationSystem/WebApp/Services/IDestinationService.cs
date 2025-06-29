using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Interface for destination-related operations
    /// </summary>
    public interface IDestinationService
    {
        /// <summary>
        /// Get all available destinations
        /// </summary>
        Task<List<DestinationModel>> GetAllDestinationsAsync();
        
        /// <summary>
        /// Get a specific destination by ID
        /// </summary>
        Task<DestinationModel?> GetDestinationByIdAsync(int id);
        
        /// <summary>
        /// Create a new destination (admin only)
        /// </summary>
        Task<DestinationModel?> CreateDestinationAsync(DestinationModel destination);
        
        /// <summary>
        /// Update an existing destination (admin only)
        /// </summary>
        Task<DestinationModel?> UpdateDestinationAsync(int id, DestinationModel destination);
        
        /// <summary>
        /// Delete a destination (admin only)
        /// </summary>
        Task<bool> DeleteDestinationAsync(int id);
    }
} 