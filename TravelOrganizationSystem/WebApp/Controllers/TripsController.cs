using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Controllers
{
    // Test deployment - WebApp workflow trigger
    /// <summary>
    /// API Controller for handling AJAX requests from trip pages
    /// Provides paginated trip data and filtering capabilities
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;
        private readonly ILogger<TripsController> _logger;

        public TripsController(
            ITripService tripService,
            IDestinationService destinationService,
            ILogger<TripsController> logger)
        {
            _tripService = tripService;
            _destinationService = destinationService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated trips with optional destination filtering
        /// Used for AJAX pagination and filtering
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="destinationId">Optional destination ID for filtering</param>
        /// <returns>Paginated trip data with metadata</returns>
        [HttpGet]
        public async Task<IActionResult> GetTrips(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? destinationId = null)
        {
            try
            {
                _logger.LogInformation("AJAX request for trips - Page: {Page}, PageSize: {PageSize}, DestinationId: {DestinationId}", 
                    page, pageSize, destinationId);

                // Validate parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 50) pageSize = 10;

                // Get trips with pagination
                var (trips, totalCount) = await _tripService.GetTripsAsync(page, pageSize, destinationId);

                // Calculate pagination metadata
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var hasNextPage = page < totalPages;
                var hasPreviousPage = page > 1;

                // Prepare response
                var response = new
                {
                    trips = trips,
                    pagination = new
                    {
                        currentPage = page,
                        pageSize = pageSize,
                        totalItems = totalCount,
                        totalPages = totalPages,
                        hasNextPage = hasNextPage,
                        hasPreviousPage = hasPreviousPage,
                        startItem = ((page - 1) * pageSize) + 1,
                        endItem = Math.Min(page * pageSize, totalCount)
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling AJAX trips request");
                return StatusCode(500, new { message = "An error occurred while loading trips", details = ex.Message });
            }
        }

        /// <summary>
        /// Get available destinations for filter dropdown
        /// Used for AJAX requests when refreshing filters
        /// </summary>
        /// <returns>List of destinations</returns>
        [HttpGet("destinations")]
        public async Task<IActionResult> GetDestinations()
        {
            try
            {
                var destinations = await _destinationService.GetAllDestinationsAsync();
                return Ok(destinations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading destinations for filter");
                return StatusCode(500, new { message = "Error loading destinations" });
            }
        }
    }
} 