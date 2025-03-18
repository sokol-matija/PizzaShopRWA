using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AllergenController : ControllerBase
	{
		private readonly IAllergenService _allergenService;

		public AllergenController(IAllergenService allergenService)
		{
			_allergenService = allergenService;
		}

		// GET: api/allergen
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var allergens = await _allergenService.GetAllAsync();
			return Ok(allergens);
		}

		// GET: api/allergen/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var allergen = await _allergenService.GetByIdAsync(id);
			if (allergen == null)
				return NotFound();

			return Ok(allergen);
		}

		// POST: api/allergen
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([FromBody] AllergenCreateDTO allergenDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var createdAllergen = await _allergenService.CreateAsync(allergenDto);
			if (createdAllergen == null)
				return BadRequest("An allergen with the same name already exists");

			return CreatedAtAction(nameof(GetById), new { id = createdAllergen.Id }, createdAllergen);
		}

		// PUT: api/allergen/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Update(int id, [FromBody] AllergenUpdateDTO allergenDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var updatedAllergen = await _allergenService.UpdateAsync(id, allergenDto);
			if (updatedAllergen == null)
				return NotFound();

			return Ok(updatedAllergen);
		}

		// DELETE: api/allergen/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _allergenService.DeleteAsync(id);
			if (!result)
				return NotFound();

			return NoContent();
		}
	}
}