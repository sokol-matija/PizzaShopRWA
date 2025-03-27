using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing travel destinations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DestinationController : ControllerBase
    {
        private readonly IDestinationService _destinationService;

        public DestinationController(IDestinationService destinationService)
        {
            _destinationService = destinationService;
        }

        /// <summary>
        /// Get all available destinations
        /// </summary>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>List of all destinations</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Destination>>> GetAllDestinations()
        {
            var destinations = await _destinationService.GetAllDestinationsAsync();
            return Ok(destinations);
        }

        /// <summary>
        /// Get a specific destination by ID
        /// </summary>
        /// <param name="id">The destination ID to retrieve</param>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>Destination details if found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Destination>> GetDestination(int id)
        {
            var destination = await _destinationService.GetDestinationByIdAsync(id);
            if (destination == null)
                return NotFound();

            return Ok(destination);
        }

        /// <summary>
        /// Create a new destination
        /// </summary>
        /// <param name="destination">The destination details to create</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The newly created destination</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Destination>> CreateDestination(Destination destination)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdDestination = await _destinationService.CreateDestinationAsync(destination);
            return CreatedAtAction(nameof(GetDestination), new { id = createdDestination.Id }, createdDestination);
        }

        /// <summary>
        /// Update an existing destination
        /// </summary>
        /// <param name="id">The ID of the destination to update</param>
        /// <param name="destination">The updated destination details</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The updated destination</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Destination>> UpdateDestination(int id, Destination destination)
        {
            if (id != destination.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedDestination = await _destinationService.UpdateDestinationAsync(id, destination);
            if (updatedDestination == null)
                return NotFound();

            return Ok(updatedDestination);
        }

        /// <summary>
        /// Delete a destination
        /// </summary>
        /// <param name="id">The ID of the destination to delete</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>No content if deletion is successful</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteDestination(int id)
        {
            var result = await _destinationService.DeleteDestinationAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
} 