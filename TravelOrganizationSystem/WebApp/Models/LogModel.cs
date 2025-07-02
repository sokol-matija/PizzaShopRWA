using System;

namespace WebApp.Models
{
    /// <summary>
    /// Model representing a system log entry for the WebApp
    /// Used to display log information in the admin interface
    /// </summary>
    public class LogModel
    {
        /// <summary>
        /// Unique identifier for the log entry
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Timestamp when the log entry was created
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Log level (Info, Warning, Error, etc.)
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Log message content
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Formatted timestamp for display purposes
        /// </summary>
        public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// CSS class for the log level badge
        /// </summary>
        public string LevelBadgeClass => Level?.ToLower() switch
        {
            "error" => "bg-danger",
            "warning" => "bg-warning text-dark",
            "info" => "bg-info",
            "debug" => "bg-secondary",
            _ => "bg-primary"
        };

        /// <summary>
        /// Icon for the log level
        /// </summary>
        public string LevelIcon => Level?.ToLower() switch
        {
            "error" => "fas fa-exclamation-circle",
            "warning" => "fas fa-exclamation-triangle",
            "info" => "fas fa-info-circle",
            "debug" => "fas fa-bug",
            _ => "fas fa-file-alt"
        };
    }
} 