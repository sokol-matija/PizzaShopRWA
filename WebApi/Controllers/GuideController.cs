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
    public class GuideController : ControllerBase
    {
        private readonly IGuideService _guideService;

        public GuideController(IGuideService guideService)
        {
            _guideService = guideService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Guide>>> GetAllGuides()
        {
            var guides = await _guideService.GetAllGuidesAsync();
            return Ok(guides);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Guide>> GetGuide(int id)
        {
            var guide = await _guideService.GetGuideByIdAsync(id);
            if (guide == null)
                return NotFound();

            return Ok(guide);
        }

        [HttpGet("trip/{tripId}")]
        public async Task<ActionResult<IEnumerable<Guide>>> GetGuidesByTrip(int tripId)
        {
            var guides = await _guideService.GetGuidesByTripAsync(tripId);
            return Ok(guides);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Guide>> CreateGuide(Guide guide)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdGuide = await _guideService.CreateGuideAsync(guide);
            return CreatedAtAction(nameof(GetGuide), new { id = createdGuide.Id }, createdGuide);
        }

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