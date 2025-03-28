using Microsoft.AspNetCore.Mvc;
using TravelOrganizationWebApp.Services;

namespace TravelOrganizationWebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnsplashController : ControllerBase
    {
        private readonly IUnsplashService _unsplashService;
        private readonly ILogger<UnsplashController> _logger;

        public UnsplashController(IUnsplashService unsplashService, ILogger<UnsplashController> logger)
        {
            _unsplashService = unsplashService;
            _logger = logger;
        }

        /// <summary>
        /// Get a random image from Unsplash based on a search query
        /// </summary>
        /// <param name="query">The search term to use for finding a relevant image</param>
        /// <returns>A random image URL and related metadata</returns>
        [HttpGet("random")]
        public async Task<IActionResult> GetRandomImage([FromQuery] string query)
        {
            _logger.LogInformation("Received request for Unsplash image with query: {Query}", query);
            
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Query parameter is empty or missing");
                return BadRequest("Query parameter is required");
            }

            try
            {
                _logger.LogInformation("Calling Unsplash service with query: {Query}", query);
                var imageUrl = await _unsplashService.GetRandomImageUrlAsync(query);
                
                if (string.IsNullOrEmpty(imageUrl))
                {
                    _logger.LogWarning("No image found for query: {Query}", query);
                    return NotFound("No image found for the given query");
                }

                _logger.LogInformation("Successfully retrieved image for query: {Query}", query);
                return Ok(new { imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching image for query: {Query}", query);
                return StatusCode(500, $"An error occurred while fetching the image: {ex.Message}");
            }
        }
    }
} 