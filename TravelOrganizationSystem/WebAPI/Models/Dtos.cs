using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    /// <summary>
    /// DTO for image URL update requests
    /// </summary>
    public class ImageUrlUpdateRequest
    {
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
    }
} 