using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    // Add models for authentication
    public class TokenResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public string ExpiresAt { get; set; } = string.Empty;
    }
} 