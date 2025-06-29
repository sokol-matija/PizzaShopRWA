using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for managing travel guides
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class GuideController : ControllerBase
    {
        private readonly IGuideService _guideService;

        public GuideController(IGuideService guideService)
        {
            _guideService = guideService;
        }

        /// <summary>
        /// Get all available guides
        /// </summary>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>List of all guides</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Guide>>> GetAllGuides()
        {
            var guides = await _guideService.GetAllGuidesAsync();
            return Ok(guides);
        }

        /// <summary>
        /// Get a specific guide by ID
        /// </summary>
        /// <param name="id">The guide ID to retrieve</param>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>Guide details if found</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Guide>> GetGuide(int id)
        {
            var guide = await _guideService.GetGuideByIdAsync(id);
            if (guide == null)
                return NotFound();

            return Ok(guide);
        }

        /// <summary>
        /// Get all guides assigned to a specific trip
        /// </summary>
        /// <param name="tripId">The trip ID to get guides for</param>
        /// <remarks>
        /// This endpoint is publicly accessible - no authentication required
        /// </remarks>
        /// <returns>List of guides assigned to the specified trip</returns>
        [HttpGet("trip/{tripId}")]
        public async Task<ActionResult<IEnumerable<Guide>>> GetGuidesByTrip(int tripId)
        {
            var guides = await _guideService.GetGuidesByTripAsync(tripId);
            return Ok(guides);
        }

        /// <summary>
        /// Create a new guide
        /// </summary>
        /// <param name="guide">The guide details to create</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The newly created guide</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guide>> CreateGuide(Guide guide)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdGuide = await _guideService.CreateGuideAsync(guide);
            return CreatedAtAction(nameof(GetGuide), new { id = createdGuide.Id }, createdGuide);
        }

        /// <summary>
        /// Update an existing guide
        /// </summary>
        /// <param name="id">The ID of the guide to update</param>
        /// <param name="guide">The updated guide details</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>The updated guide</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guide>> UpdateGuide(int id, Guide guide)
        {
            if (id != guide.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedGuide = await _guideService.UpdateGuideAsync(id, guide);
            if (updatedGuide == null)
                return NotFound();

            return Ok(updatedGuide);
        }

        /// <summary>
        /// Delete a guide
        /// </summary>
        /// <param name="id">The ID of the guide to delete</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>No content if deletion is successful</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteGuide(int id)
        {
            var result = await _guideService.DeleteGuideAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
} 