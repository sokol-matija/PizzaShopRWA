using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FoodController : ControllerBase
	{
		private readonly IFoodService _foodService;
		private readonly ILogService _logService;

		public FoodController(IFoodService foodService, ILogService logService)
		{
			_foodService = foodService;
			_logService = logService;
		}

		// GET: api/food?page=1&count=10
		[HttpGet]
		public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int count = 10)
		{
			if (page < 1 || count < 1)
				return BadRequest("Page and count must be greater than 0");

			var pagedFoods = await _foodService.GetAllAsync(page, count);
			return Ok(pagedFoods);
		}

		// GET: api/food/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var food = await _foodService.GetByIdAsync(id);
			if (food == null)
				return NotFound();

			return Ok(food);
		}

		// POST: api/food
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([FromBody] FoodCreateDTO foodDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var createdFood = await _foodService.CreateAsync(foodDto);
			if (createdFood == null)
				return BadRequest("A food with the same name already exists");

			return CreatedAtAction(nameof(GetById), new { id = createdFood.Id }, createdFood);
		}

		// PUT: api/food/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Update(int id, [FromBody] FoodUpdateDTO foodDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var updatedFood = await _foodService.UpdateAsync(id, foodDto);
			if (updatedFood == null)
				return NotFound();

			return Ok(updatedFood);
		}

		// DELETE: api/food/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _foodService.DeleteAsync(id);
			if (!result)
				return NotFound();

			return NoContent();
		}

		// GET: api/food/search
		[HttpGet("search")]
		public async Task<IActionResult> Search([FromQuery] FoodSearchDTO searchParams)
		{
			if (searchParams.Page < 1 || searchParams.Count < 1)
				return BadRequest("Page and count must be greater than 0");

			var searchResults = await _foodService.SearchAsync(searchParams);
			return Ok(searchResults);
		}
	}
}