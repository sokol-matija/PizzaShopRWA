using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Services;
using WebAPI.DTOs;
using System;
using System.Linq;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing trip registrations (bookings)
    /// </summary>
    /// <remarks>
    /// Base authentication required for all endpoints in this controller
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TripRegistrationController : ControllerBase
    {
        private readonly ITripRegistrationService _registrationService;

        public TripRegistrationController(ITripRegistrationService registrationService)
        {
            _registrationService = registrationService;
        }

        /// <summary>
        /// Get all trip registrations in the system
        /// </summary>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>List of all trip registrations</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TripRegistrationDTO>>> GetAllRegistrations()
        {
            var registrations = await _registrationService.GetAllRegistrationsAsync();
            var dtos = registrations.Select(MapRegistrationToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get a specific trip registration by ID
        /// </summary>
        /// <param name="id">The registration ID to retrieve</param>
        /// <remarks>
        /// Regular users can only access their own registrations
        /// Admins can access any registration
        /// </remarks>
        /// <returns>Registration details if found and authorized</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TripRegistrationDTO>> GetRegistration(int id)
        {
            var registration = await _registrationService.GetRegistrationByIdAsync(id);
            if (registration == null)
                return NotFound();

            // Check if the user is authorized to view this registration
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Admin") && registration.UserId != userId)
                return Forbid();

            return Ok(MapRegistrationToDto(registration));
        }

        /// <summary>
        /// Get all registrations for a specific user
        /// </summary>
        /// <param name="userId">The user ID to get registrations for</param>
        /// <remarks>
        /// Regular users can only access their own registrations
        /// Admins can access any user's registrations
        /// </remarks>
        /// <returns>List of registrations for the specified user if authorized</returns>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TripRegistrationDTO>>> GetRegistrationsByUser(int userId)
        {
            // Check if the user is authorized to view these registrations
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Admin") && userId != currentUserId)
                return Forbid();

            var registrations = await _registrationService.GetRegistrationsByUserAsync(userId);
            var dtos = registrations.Select(MapRegistrationToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get all registrations for a specific trip
        /// </summary>
        /// <param name="tripId">The trip ID to get registrations for</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>List of registrations for the specified trip</returns>
        [HttpGet("trip/{tripId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TripRegistrationDTO>>> GetRegistrationsByTrip(int tripId)
        {
            var registrations = await _registrationService.GetRegistrationsByTripAsync(tripId);
            var dtos = registrations.Select(MapRegistrationToDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Create a new trip registration (book a trip)
        /// </summary>
        /// <param name="registrationDto">The registration details to create</param>
        /// <remarks>
        /// Regular users can only create registrations for themselves
        /// Admins can create registrations for any user
        /// </remarks>
        /// <returns>The newly created registration</returns>
        [HttpPost]
        public async Task<ActionResult<TripRegistrationDTO>> CreateRegistration(CreateTripRegistrationDTO registrationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Create a new TripRegistration entity from the DTO
            var registration = new TripRegistration
            {
                TripId = registrationDto.TripId,
                NumberOfParticipants = registrationDto.NumberOfParticipants,
                RegistrationDate = DateTime.Now,
                Status = "Pending" // Default status for new registrations
            };

            // Set the user ID to the current user if not specified and not admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (registrationDto.UserId.HasValue && User.IsInRole("Admin"))
            {
                registration.UserId = registrationDto.UserId.Value;
            }
            else
            {
                registration.UserId = currentUserId;
            }

            var createdRegistration = await _registrationService.CreateRegistrationAsync(registration);
            if (createdRegistration == null)
                return BadRequest("Unable to create registration. The trip may be full or not exist.");

            return CreatedAtAction(nameof(GetRegistration), new { id = createdRegistration.Id }, MapRegistrationToDto(createdRegistration));
        }

        /// <summary>
        /// Update an existing trip registration
        /// </summary>
        /// <param name="id">The ID of the registration to update</param>
        /// <param name="registrationDto">The updated registration details</param>
        /// <remarks>
        /// Regular users can only update their own registrations
        /// Admins can update any registration
        /// </remarks>
        /// <returns>The updated registration</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<TripRegistrationDTO>> UpdateRegistration(int id, UpdateTripRegistrationDTO registrationDto)
        {
            if (id != registrationDto.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if the user is authorized to update this registration
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var existingRegistration = await _registrationService.GetRegistrationByIdAsync(id);
            if (existingRegistration == null)
                return NotFound();

            if (!User.IsInRole("Admin") && existingRegistration.UserId != currentUserId)
                return Forbid();

            // Create a TripRegistration entity from the DTO
            var registration = new TripRegistration
            {
                Id = registrationDto.Id,
                UserId = existingRegistration.UserId,
                TripId = existingRegistration.TripId,
                RegistrationDate = existingRegistration.RegistrationDate,
                NumberOfParticipants = registrationDto.NumberOfParticipants,
                Status = registrationDto.Status
            };

            var updatedRegistration = await _registrationService.UpdateRegistrationAsync(id, registration);
            if (updatedRegistration == null)
                return BadRequest("Unable to update registration. The trip may be full.");

            return Ok(MapRegistrationToDto(updatedRegistration));
        }

        /// <summary>
        /// Delete a trip registration (cancel a booking)
        /// </summary>
        /// <param name="id">The ID of the registration to delete</param>
        /// <remarks>
        /// Regular users can only delete their own registrations
        /// Admins can delete any registration
        /// </remarks>
        /// <returns>No content if deletion is successful</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRegistration(int id)
        {
            // Check if the user is authorized to delete this registration
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var registration = await _registrationService.GetRegistrationByIdAsync(id);
            if (registration == null)
                return NotFound();

            if (!User.IsInRole("Admin") && registration.UserId != currentUserId)
                return Forbid();

            var result = await _registrationService.DeleteRegistrationAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Update the status of a trip registration
        /// </summary>
        /// <param name="id">The ID of the registration to update</param>
        /// <param name="status">The new registration status</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>No content if status update is successful</returns>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UpdateRegistrationStatus(int id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status))
                return BadRequest("Status cannot be empty");

            var result = await _registrationService.UpdateRegistrationStatusAsync(id, status);
            if (!result)
                return NotFound();

            return NoContent();
        }

        // Helper method to map TripRegistration entity to TripRegistrationDTO
        private TripRegistrationDTO MapRegistrationToDto(TripRegistration registration)
        {
            return new TripRegistrationDTO
            {
                Id = registration.Id,
                UserId = registration.UserId,
                Username = registration.User?.Username ?? string.Empty,
                TripId = registration.TripId,
                TripName = registration.Trip?.Name ?? string.Empty,
                DestinationName = registration.Trip?.Destination?.Name ?? string.Empty,
                StartDate = registration.Trip?.StartDate ?? DateTime.MinValue,
                EndDate = registration.Trip?.EndDate ?? DateTime.MinValue,
                RegistrationDate = registration.RegistrationDate,
                NumberOfParticipants = registration.NumberOfParticipants,
                TotalPrice = registration.TotalPrice,
                Status = registration.Status ?? string.Empty
            };
        }
    }
} 