using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Services;
using WebAPI.DTOs;
using System.Linq;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing travel trips
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
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
                // Always use the destination's image URL since trips don't have their own images
                ImageUrl = trip.Destination?.ImageUrl ?? string.Empty,
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
                    ImageUrl = tg.Guide.ImageUrl ?? string.Empty,
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
    }
} 