using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TripRegistration>>> GetAllRegistrations()
        {
            var registrations = await _registrationService.GetAllRegistrationsAsync();
            return Ok(registrations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TripRegistration>> GetRegistration(int id)
        {
            var registration = await _registrationService.GetRegistrationByIdAsync(id);
            if (registration == null)
                return NotFound();

            // Check if the user is authorized to view this registration
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Admin") && registration.UserId != userId)
                return Forbid();

            return Ok(registration);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TripRegistration>>> GetRegistrationsByUser(int userId)
        {
            // Check if the user is authorized to view these registrations
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Admin") && userId != currentUserId)
                return Forbid();

            var registrations = await _registrationService.GetRegistrationsByUserAsync(userId);
            return Ok(registrations);
        }

        [HttpGet("trip/{tripId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<TripRegistration>>> GetRegistrationsByTrip(int tripId)
        {
            var registrations = await _registrationService.GetRegistrationsByTripAsync(tripId);
            return Ok(registrations);
        }

        [HttpPost]
        public async Task<ActionResult<TripRegistration>> CreateRegistration(TripRegistration registration)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Set the user ID to the current user if not specified and not admin
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (!User.IsInRole("Admin"))
                registration.UserId = currentUserId;

            var createdRegistration = await _registrationService.CreateRegistrationAsync(registration);
            if (createdRegistration == null)
                return BadRequest("Unable to create registration. The trip may be full or not exist.");

            return CreatedAtAction(nameof(GetRegistration), new { id = createdRegistration.Id }, createdRegistration);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TripRegistration>> UpdateRegistration(int id, TripRegistration registration)
        {
            if (id != registration.Id)
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

            var updatedRegistration = await _registrationService.UpdateRegistrationAsync(id, registration);
            if (updatedRegistration == null)
                return BadRequest("Unable to update registration. The trip may be full.");

            return Ok(updatedRegistration);
        }

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
    }
} 