using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Service interface for managing travel guides
    /// Provides AJAX-friendly methods for CRUD operations
    /// </summary>
    public interface IGuideService
    {
        /// <summary>
        /// Get all available guides
        /// </summary>
        Task<IEnumerable<GuideModel>> GetAllGuidesAsync();

        /// <summary>
        /// Get a specific guide by ID
        /// </summary>
        /// <param name="id">Guide ID to retrieve</param>
        Task<GuideModel?> GetGuideByIdAsync(int id);

        /// <summary>
        /// Create a new guide
        /// </summary>
        /// <param name="guide">Guide data to create</param>
        Task<GuideModel?> CreateGuideAsync(GuideModel guide);

        /// <summary>
        /// Update an existing guide
        /// </summary>
        /// <param name="id">Guide ID to update</param>
        /// <param name="guide">Updated guide data</param>
        Task<GuideModel?> UpdateGuideAsync(int id, GuideModel guide);

        /// <summary>
        /// Delete a guide
        /// </summary>
        /// <param name="id">Guide ID to delete</param>
        Task<bool> DeleteGuideAsync(int id);

        /// <summary>
        /// Get all guides assigned to a specific trip
        /// </summary>
        /// <param name="tripId">Trip ID to get guides for</param>
        Task<IEnumerable<GuideModel>> GetGuidesByTripAsync(int tripId);
    }
} 