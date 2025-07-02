using WebApp.Models;

namespace WebApp.Services
{
    /// <summary>
    /// Interface for managing system logs
    /// Provides methods to retrieve log entries and counts for admin interface
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Get the most recent log entries up to the specified count
        /// </summary>
        /// <param name="count">Number of log entries to retrieve</param>
        /// <returns>List of log entries ordered by timestamp descending</returns>
        Task<List<LogModel>> GetLogsAsync(int count);

        /// <summary>
        /// Get the total count of log entries in the system
        /// </summary>
        /// <returns>Total number of log entries</returns>
        Task<int> GetLogsCountAsync();

        /// <summary>
        /// Get logs with pagination support
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of logs per page</param>
        /// <returns>Tuple containing the logs and total count</returns>
        Task<(List<LogModel> logs, int totalCount)> GetLogsAsync(int page = 1, int pageSize = 50);
    }
} 