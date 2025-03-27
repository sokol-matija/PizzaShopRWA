using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Services;

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
        public async Task<ActionResult<IEnumerable<Trip>>> GetAllTrips()
        {
            var trips = await _tripService.GetAllTripsAsync();
            return Ok(trips);
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
        public async Task<ActionResult<Trip>> GetTrip(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);
            if (trip == null)
                return NotFound();

            return Ok(trip);
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
        public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByDestination(int destinationId)
        {
            var trips = await _tripService.GetTripsByDestinationAsync(destinationId);
            return Ok(trips);
        }

        /// <summary>
        /// Create a new trip
        /// </summary>
        /// <param name="trip">The trip details to create</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The newly created trip</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Trip>> CreateTrip(Trip trip)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTrip = await _tripService.CreateTripAsync(trip);
            return CreatedAtAction(nameof(GetTrip), new { id = createdTrip.Id }, createdTrip);
        }

        /// <summary>
        /// Update an existing trip
        /// </summary>
        /// <param name="id">The ID of the trip to update</param>
        /// <param name="trip">The updated trip details</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The updated trip</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Trip>> UpdateTrip(int id, Trip trip)
        {
            if (id != trip.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedTrip = await _tripService.UpdateTripAsync(id, trip);
            if (updatedTrip == null)
                return NotFound();

            return Ok(updatedTrip);
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