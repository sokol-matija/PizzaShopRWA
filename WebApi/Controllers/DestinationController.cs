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
    /// Controller for managing destinations
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
        /// Get all destinations
        /// </summary>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>List of all destinations</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DestinationDTO>>> GetAllDestinations()
        {
            var destinations = await _destinationService.GetAllDestinationsAsync();
            var destinationDtos = destinations.Select(MapDestinationToDto).ToList();
            return Ok(destinationDtos);
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
        public async Task<ActionResult<DestinationDTO>> GetDestination(int id)
        {
            var destination = await _destinationService.GetDestinationByIdAsync(id);
            if (destination == null)
                return NotFound();

            return Ok(MapDestinationToDto(destination));
        }

        /// <summary>
        /// Create a new destination
        /// </summary>
        /// <param name="destinationDto">The destination details to create</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The newly created destination</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DestinationDTO>> CreateDestination(CreateDestinationDTO destinationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var destination = new Destination
            {
                Name = destinationDto.Name,
                Description = destinationDto.Description,
                Country = destinationDto.Country,
                City = destinationDto.City,
                ImageUrl = destinationDto.ImageUrl
            };

            var createdDestination = await _destinationService.CreateDestinationAsync(destination);
            return CreatedAtAction(nameof(GetDestination), new { id = createdDestination.Id }, 
                MapDestinationToDto(createdDestination));
        }

        /// <summary>
        /// Update an existing destination
        /// </summary>
        /// <param name="id">The ID of the destination to update</param>
        /// <param name="destinationDto">The updated destination details</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The updated destination</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DestinationDTO>> UpdateDestination(int id, UpdateDestinationDTO destinationDto)
        {
            if (id != destinationDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Map DTO to entity
            var destination = new Destination
            {
                Id = destinationDto.Id,
                Name = destinationDto.Name,
                Description = destinationDto.Description,
                Country = destinationDto.Country,
                City = destinationDto.City,
                ImageUrl = destinationDto.ImageUrl
            };

            var updatedDestination = await _destinationService.UpdateDestinationAsync(id, destination);
            if (updatedDestination == null)
                return NotFound();

            return Ok(MapDestinationToDto(updatedDestination));
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

        /// <summary>
        /// Update a destination's image URL
        /// </summary>
        /// <param name="id">The ID of the destination to update</param>
        /// <param name="imageUrl">The new image URL</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>No content if update is successful</returns>
        [HttpPut("{id}/image")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateDestinationImage(int id, [FromBody] UpdateDestinationImageDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get the destination first
            var destination = await _destinationService.GetDestinationByIdAsync(id);
            if (destination == null)
                return NotFound();

            // Update the image URL
            destination.ImageUrl = model.ImageUrl;
            
            // Save the changes
            var result = await _destinationService.UpdateDestinationAsync(id, destination);
            if (result == null)
                return BadRequest("Failed to update destination image");

            return NoContent();
        }

        // Helper method to map Destination entity to DestinationDTO
        private DestinationDTO MapDestinationToDto(Destination destination)
        {
            return new DestinationDTO
            {
                Id = destination.Id,
                Name = destination.Name,
                Description = destination.Description ?? string.Empty,
                Country = destination.Country,
                City = destination.City,
                ImageUrl = destination.ImageUrl
            };
        }
    }

    // DTO for updating destination image
    public class UpdateDestinationImageDTO
    {
        public string ImageUrl { get; set; }
    }
} 