using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Services
{
	public interface IFoodCategoryService
	{
		Task<List<FoodCategoryDTO>> GetAllAsync();
		Task<FoodCategoryDTO> GetByIdAsync(int id);
		Task<FoodCategoryDTO> CreateAsync(FoodCategoryCreateDTO categoryDto);
		Task<FoodCategoryDTO> UpdateAsync(int id, FoodCategoryUpdateDTO categoryDto);
		Task<bool> DeleteAsync(int id);
	}

	public class FoodCategoryService : IFoodCategoryService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogService _logService;

		public FoodCategoryService(ApplicationDbContext context, ILogService logService)
		{
			_context = context;
			_logService = logService;
		}

		public async Task<List<FoodCategoryDTO>> GetAllAsync()
		{
			var categories = await _context.FoodCategories
				.OrderBy(c => c.Name)
				.ToListAsync();

			var categoryDtos = categories.Select(c => new FoodCategoryDTO
			{
				Id = c.Id,
				Name = c.Name,
				Description = c.Description
			}).ToList();

			await _logService.LogInformationAsync($"Retrieved all {categoryDtos.Count} food categories");
			return categoryDtos;
		}

		public async Task<FoodCategoryDTO> GetByIdAsync(int id)
		{
			var category = await _context.FoodCategories.FindAsync(id);
			if (category == null)
			{
				await _logService.LogWarningAsync($"Food category with id={id} not found");
				return null;
			}

			await _logService.LogInformationAsync($"Retrieved food category with id={id}");
			return new FoodCategoryDTO
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description
			};
		}

		public async Task<FoodCategoryDTO> CreateAsync(FoodCategoryCreateDTO categoryDto)
		{
			// Check if category with same name already exists
			if (await _context.FoodCategories.AnyAsync(c => c.Name == categoryDto.Name))
			{
				await _logService.LogWarningAsync($"Cannot create food category: category with name '{categoryDto.Name}' already exists");
				return null;
			}

			// Create new category
			var category = new FoodCategory
			{
				Name = categoryDto.Name,
				Description = categoryDto.Description
			};

			_context.FoodCategories.Add(category);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Food category with id={category.Id} was created");
			return new FoodCategoryDTO
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description
			};
		}

		public async Task<FoodCategoryDTO> UpdateAsync(int id, FoodCategoryUpdateDTO categoryDto)
		{
			// Find the category to update
			var category = await _context.FoodCategories.FindAsync(id);
			if (category == null)
			{
				await _logService.LogWarningAsync($"Cannot update food category: category with id={id} not found");
				return null;
			}

			// Check if name is being changed and if it would conflict
			if (category.Name != categoryDto.Name && await _context.FoodCategories.AnyAsync(c => c.Name == categoryDto.Name && c.Id != id))
			{
				await _logService.LogWarningAsync($"Cannot update food category: category with name '{categoryDto.Name}' already exists");
				return null;
			}

			// Update properties
			category.Name = categoryDto.Name;
			category.Description = categoryDto.Description;

			// Save changes
			try
			{
				await _context.SaveChangesAsync();
				await _logService.LogInformationAsync($"Food category with id={id} was updated");
			}
			catch (Exception ex)
			{
				await _logService.LogErrorAsync($"Error updating food category with id={id}: {ex.Message}");
				return null;
			}

			return new FoodCategoryDTO
			{
				Id = category.Id,
				Name = category.Name,
				Description = category.Description
			};
		}

		public async Task<bool> DeleteAsync(int id)
		{
			// Find the category to delete
			var category = await _context.FoodCategories.FindAsync(id);
			if (category == null)
			{
				await _logService.LogWarningAsync($"Cannot delete food category: category with id={id} not found");
				return false;
			}

			// Check if category is used by any foods
			var isUsedByFoods = await _context.Foods.AnyAsync(f => f.FoodCategoryId == id);
			if (isUsedByFoods)
			{
				await _logService.LogWarningAsync($"Cannot delete food category with id={id}: it is used by food items");
				return false;
			}

			// Delete category
			_context.FoodCategories.Remove(category);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Food category with id={id} was deleted");
			return true;
		}
	}
}