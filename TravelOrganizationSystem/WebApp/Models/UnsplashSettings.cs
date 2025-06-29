namespace WebApp.Models
{
    public class UnsplashSettings
    {
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string ApplicationId { get; set; } = string.Empty;
        public int CacheDurationMinutes { get; set; } = 60; // Default cache duration
    }
} 