using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Trip>>> GetAllTrips()
        {
            var trips = await _tripService.GetAllTripsAsync();
            return Ok(trips);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Trip>> GetTrip(int id)
        {
            var trip = await _tripService.GetTripByIdAsync(id);
            if (trip == null)
                return NotFound();

            return Ok(trip);
        }

        [HttpGet("destination/{destinationId}")]
        public async Task<ActionResult<IEnumerable<Trip>>> GetTripsByDestination(int destinationId)
        {
            var trips = await _tripService.GetTripsByDestinationAsync(destinationId);
            return Ok(trips);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Trip>> CreateTrip(Trip trip)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTrip = await _tripService.CreateTripAsync(trip);
            return CreatedAtAction(nameof(GetTrip), new { id = createdTrip.Id }, createdTrip);
        }

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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteTrip(int id)
        {
            var result = await _tripService.DeleteTripAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{tripId}/guides/{guideId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignGuideToTrip(int tripId, int guideId)
        {
            var result = await _tripService.AssignGuideToTripAsync(tripId, guideId);
            if (!result)
                return NotFound();

            return NoContent();
        }

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