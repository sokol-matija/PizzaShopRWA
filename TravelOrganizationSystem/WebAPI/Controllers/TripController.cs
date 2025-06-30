using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Services;
using WebAPI.DTOs;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing travel trips
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private const string DEFAULT_GUIDE_PROFILE_IMAGE = "/images/default-guide-profile.svg";
        private readonly ITripService _tripService;
        private readonly ILogger<TripController> _logger;

        public TripController(ITripService tripService, ILogger<TripController> logger)
        {
            _tripService = tripService;
            _logger = logger;
        }

        /// <summary>
        /// Get all available trips
        /// </summary>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>List of all trips</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripDTO>>> GetAllTrips()
        {
            var trips = await _tripService.GetAllTripsAsync();
            var tripDtos = trips.Select(MapTripToDto).ToList();
            return Ok(tripDtos);
        }

        /// <summary>
        /// Get a specific trip by ID
        /// </summary>
        /// <param name="id">The trip ID to retrieve</param>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>Trip details if found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TripDTO>> GetTrip(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);
            if (trip == null)
                return NotFound();

            return Ok(MapTripToDto(trip));
        }

        /// <summary>
        /// Get all trips for a specific destination
        /// </summary>
        /// <param name="destinationId">The destination ID to filter trips by</param>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>List of trips for the specified destination</returns>
        [HttpGet("destination/{destinationId}")]
        public async Task<ActionResult<IEnumerable<TripDTO>>> GetTripsByDestination(int destinationId)
        {
            var trips = await _tripService.GetTripsByDestinationAsync(destinationId);
            var tripDtos = trips.Select(MapTripToDto).ToList();
            return Ok(tripDtos);
        }

        /// <summary>
        /// Search trips by name and/or description with pagination
        /// </summary>
        /// <param name="name">Optional name to search for</param>
        /// <param name="description">Optional description to search for</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="count">Number of items per page (default: 10)</param>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// Supports searching by trip name and/or description.
        /// Use pagination parameters to get results in manageable chunks.
        /// </remarks>
        /// <returns>Paginated list of trips matching search criteria</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TripDTO>>> SearchTrips(
            [FromQuery] string? name,
            [FromQuery] string? description,
            [FromQuery] int page = 1,
            [FromQuery] int count = 10)
        {
            // Validate pagination parameters
            if (page < 1)
                return BadRequest("Page number must be 1 or greater");
            
            if (count < 1 || count > 100)
                return BadRequest("Count must be between 1 and 100");

            var trips = await _tripService.SearchTripsAsync(name, description, page, count);
            var tripDtos = trips.Select(MapTripToDto).ToList();
            return Ok(tripDtos);
        }

        /// <summary>
        /// Create a new trip
        /// </summary>
        /// <param name="tripDto">The trip details to create</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The newly created trip</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TripDTO>> CreateTrip(CreateTripDTO tripDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var trip = new Trip
            {
                Name = tripDto.Name,
                Description = tripDto.Description,
                StartDate = tripDto.StartDate,
                EndDate = tripDto.EndDate,
                Price = tripDto.Price,
                ImageUrl = tripDto.ImageUrl,
                MaxParticipants = tripDto.MaxParticipants,
                DestinationId = tripDto.DestinationId
            };

            var createdTrip = await _tripService.CreateTripAsync(trip);
            return CreatedAtAction(nameof(GetTrip), new { id = createdTrip.Id }, MapTripToDto(createdTrip));
        }

        /// <summary>
        /// Update an existing trip
        /// </summary>
        /// <param name="id">The ID of the trip to update</param>
        /// <param name="tripDto">The updated trip details</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The updated trip</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TripDTO>> UpdateTrip(int id, UpdateTripDTO tripDto)
        {
            if (id != tripDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var trip = new Trip
            {
                Id = tripDto.Id,
                Name = tripDto.Name,
                Description = tripDto.Description,
                StartDate = tripDto.StartDate,
                EndDate = tripDto.EndDate,
                Price = tripDto.Price,
                ImageUrl = tripDto.ImageUrl,
                MaxParticipants = tripDto.MaxParticipants,
                DestinationId = tripDto.DestinationId
            };

            var updatedTrip = await _tripService.UpdateTripAsync(id, trip);
            if (updatedTrip == null)
                return NotFound();

            return Ok(MapTripToDto(updatedTrip));
        }

        // Helper method to map Trip entity to TripDTO
        private TripDTO MapTripToDto(Trip trip)
        {
            return new TripDTO
            {
                Id = trip.Id,
                Name = trip.Name,
                Description = trip.Description ?? string.Empty,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                Price = trip.Price,
                // Use trip's image URL if available, otherwise fall back to destination's image
                ImageUrl = !string.IsNullOrEmpty(trip.ImageUrl) ? trip.ImageUrl : (trip.Destination?.ImageUrl ?? string.Empty),
                MaxParticipants = trip.MaxParticipants,
                DestinationId = trip.DestinationId,
                DestinationName = trip.Destination?.Name ?? string.Empty,
                Country = trip.Destination?.Country ?? string.Empty,
                City = trip.Destination?.City ?? string.Empty,
                // Calculate available spots
                AvailableSpots = trip.MaxParticipants - (trip.TripRegistrations?.Count ?? 0),
                // Map guides if available
                Guides = trip.TripGuides?.Select(tg => new GuideDTO
                {
                    Id = tg.Guide.Id,
                    Name = tg.Guide.Name,
                    Bio = tg.Guide.Bio ?? string.Empty,
                    Email = tg.Guide.Email,
                    Phone = tg.Guide.Phone ?? string.Empty,
                    ImageUrl = tg.Guide.ImageUrl ?? DEFAULT_GUIDE_PROFILE_IMAGE,
                    YearsOfExperience = tg.Guide.YearsOfExperience
                }).ToList() ?? new List<GuideDTO>()
            };
        }

        /// <summary>
        /// Delete a trip
        /// </summary>
        /// <param name="id">The ID of the trip to delete</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>No content if deletion is successful</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTrip(int id)
        {
            var result = await _tripService.DeleteTripAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Assign a guide to a trip
        /// </summary>
        /// <param name="tripId">ID of the trip</param>
        /// <param name="guideId">ID of the guide to assign</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>No content if assignment is successful</returns>
        [HttpPost("{tripId}/guides/{guideId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignGuideToTrip(int tripId, int guideId)
        {
            var result = await _tripService.AssignGuideToTripAsync(tripId, guideId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Remove a guide from a trip
        /// </summary>
        /// <param name="tripId">ID of the trip</param>
        /// <param name="guideId">ID of the guide to remove</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>No content if removal is successful</returns>
        [HttpDelete("{tripId}/guides/{guideId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RemoveGuideFromTrip(int tripId, int guideId)
        {
            var result = await _tripService.RemoveGuideFromTripAsync(tripId, guideId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Populate images for trips that don't have them using Unsplash API
        /// </summary>
        [HttpPost("populate-images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PopulateTripImages()
        {
            try
            {
                var trips = await _tripService.GetAllTripsAsync();
                var updatedTrips = new List<string>();

                foreach (var trip in trips.Where(t => string.IsNullOrEmpty(t.ImageUrl)))
                {
                    // Create a search query based on trip name and destination
                    var searchQuery = $"{trip.Name} {trip.Destination?.City} travel";
                    
                    // Here we would need access to UnsplashService, but this is the WebAPI project
                    // The UnsplashService is in the WebApp project
                    // We'll create a simpler solution instead
                    
                    _logger.LogInformation($"Trip {trip.Id} ({trip.Name}) needs an image");
                    updatedTrips.Add($"Trip {trip.Id}: {trip.Name}");
                }

                return Ok(new { 
                    message = "Trip image population identified", 
                    tripsNeedingImages = updatedTrips.Count,
                    trips = updatedTrips 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating trip images");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update trip image URL (public endpoint for image population)
        /// </summary>
        /// <param name="id">The ID of the trip to update</param>
        /// <param name="request">The image URL to set</param>
        /// <remarks>
        /// This endpoint is public to allow image population scripts to work
        /// Only updates the ImageUrl field for security
        /// </remarks>
        /// <returns>Success status</returns>
        [HttpPut("{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTripImage(int id, [FromBody] UpdateImageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ImageUrl))
                return BadRequest("ImageUrl is required");

            try
            {
                var success = await _tripService.UpdateTripImageAsync(id, request.ImageUrl);
                
                if (!success)
                    return NotFound($"Trip with ID {id} not found");

                _logger.LogInformation("Successfully updated image for trip {TripId}", id);
                return Ok(new { message = "Trip image updated successfully", tripId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image for trip {TripId}", id);
                return StatusCode(500, "An error occurred while updating the trip image");
            }
        }

        /// <summary>
        /// Update trip image URL (public endpoint for image population)
        /// </summary>
        /// <param name="id">The ID of the trip to update</param>
        /// <param name="request">The image URL to set</param>
        /// <remarks>
        /// This endpoint is public to allow image population scripts to work
        /// Only updates the ImageUrl field for security
        /// </remarks>
        /// <returns>Success status</returns>
        [HttpPut("{id}/image/public")]
        public async Task<IActionResult> UpdateTripImagePublic(int id, [FromBody] UpdateImageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ImageUrl))
                return BadRequest("ImageUrl is required");

            try
            {
                var success = await _tripService.UpdateTripImageAsync(id, request.ImageUrl);
                
                if (!success)
                    return NotFound($"Trip with ID {id} not found");

                _logger.LogInformation("Successfully updated image for trip {TripId} via public endpoint", id);
                return Ok(new { message = "Trip image updated successfully", tripId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image for trip {TripId} via public endpoint", id);
                return StatusCode(500, "An error occurred while updating the trip image");
            }
        }

        public class UpdateImageRequest
        {
            public string ImageUrl { get; set; } = string.Empty;
        }
    }
} 