using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FoodCategoryController : ControllerBase
	{
		private readonly IFoodCategoryService _categoryService;

		public FoodCategoryController(IFoodCategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		// GET: api/foodcategory
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var categories = await _categoryService.GetAllAsync();
			return Ok(categories);
		}

		// GET: api/foodcategory/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var category = await _categoryService.GetByIdAsync(id);
			if (category == null)
				return NotFound();

			return Ok(category);
		}

		// POST: api/foodcategory
		[HttpPost]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Create([FromBody] FoodCategoryCreateDTO categoryDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var createdCategory = await _categoryService.CreateAsync(categoryDto);
			if (createdCategory == null)
				return BadRequest("A category with the same name already exists");

			return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
		}

		// PUT: api/foodcategory/5
		[HttpPut("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Update(int id, [FromBody] FoodCategoryUpdateDTO categoryDto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var updatedCategory = await _categoryService.UpdateAsync(id, categoryDto);
			if (updatedCategory == null)
				return NotFound();

			return Ok(updatedCategory);
		}

		// DELETE: api/foodcategory/5
		[HttpDelete("{id}")]
		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _categoryService.DeleteAsync(id);
			if (!result)
				return NotFound();

			return NoContent();
		}
	}
}