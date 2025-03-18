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
	public interface IFoodService
	{
		Task<PagedResultDTO<FoodDTO>> GetAllAsync(int page, int count);
		Task<FoodDTO> GetByIdAsync(int id);
		Task<PagedResultDTO<FoodDTO>> SearchAsync(FoodSearchDTO searchParams);
		Task<FoodDTO> CreateAsync(FoodCreateDTO foodDto);
		Task<FoodDTO> UpdateAsync(int id, FoodUpdateDTO foodDto);
		Task<bool> DeleteAsync(int id);
	}

	public class FoodService : IFoodService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogService _logService;

		public FoodService(ApplicationDbContext context, ILogService logService)
		{
			_context = context;
			_logService = logService;
		}

		public async Task<PagedResultDTO<FoodDTO>> GetAllAsync(int page, int count)
		{
			// Ensure valid pagination parameters
			if (page < 1) page = 1;
			if (count < 1) count = 10;

			// Calculate total count
			var totalCount = await _context.Foods.CountAsync();

			// Calculate total pages
			var pageCount = (int)Math.Ceiling(totalCount / (double)count);

			// Get items for the requested page
			var foods = await _context.Foods
				.Include(f => f.FoodCategory)
				.Include(f => f.FoodAllergens)
				.ThenInclude(fa => fa.Allergen)
				.OrderBy(f => f.Name)
				.Skip((page - 1) * count)
				.Take(count)
				.ToListAsync();

			// Map to DTOs
			var foodDtos = foods.Select(f => MapToFoodDTO(f)).ToList();

			await _logService.LogInformationAsync($"Retrieved {foodDtos.Count} food items, page {page} of {pageCount}");

			// Return paged result
			return new PagedResultDTO<FoodDTO>
			{
				Items = foodDtos,
				TotalCount = totalCount,
				PageCount = pageCount,
				CurrentPage = page
			};
		}

		public async Task<FoodDTO> GetByIdAsync(int id)
		{
			var food = await _context.Foods
				.Include(f => f.FoodCategory)
				.Include(f => f.FoodAllergens)
				.ThenInclude(fa => fa.Allergen)
				.FirstOrDefaultAsync(f => f.Id == id);

			if (food == null)
			{
				await _logService.LogWarningAsync($"Food item with id={id} not found");
				return null;
			}

			await _logService.LogInformationAsync($"Retrieved food item with id={id}");
			return MapToFoodDTO(food);
		}

		public async Task<PagedResultDTO<FoodDTO>> SearchAsync(FoodSearchDTO searchParams)
		{
			// Ensure valid pagination parameters
			if (searchParams.Page < 1) searchParams.Page = 1;
			if (searchParams.Count < 1) searchParams.Count = 10;

			// Start with all foods
			var query = _context.Foods
				.Include(f => f.FoodCategory)
				.Include(f => f.FoodAllergens)
				.ThenInclude(fa => fa.Allergen)
				.AsQueryable();

			// Apply filters if provided
			if (!string.IsNullOrEmpty(searchParams.Name))
			{
				query = query.Where(f => f.Name.Contains(searchParams.Name));
			}

			if (!string.IsNullOrEmpty(searchParams.Description))
			{
				query = query.Where(f => f.Description.Contains(searchParams.Description));
			}

			if (searchParams.CategoryId.HasValue)
			{
				query = query.Where(f => f.FoodCategoryId == searchParams.CategoryId.Value);
			}

			// Get total count after filtering
			var totalCount = await query.CountAsync();

			// Calculate total pages
			var pageCount = (int)Math.Ceiling(totalCount / (double)searchParams.Count);

			// Apply pagination
			var foods = await query
				.OrderBy(f => f.Name)
				.Skip((searchParams.Page - 1) * searchParams.Count)
				.Take(searchParams.Count)
				.ToListAsync();

			// Map to DTOs
			var foodDtos = foods.Select(f => MapToFoodDTO(f)).ToList();

			await _logService.LogInformationAsync($"Search returned {foodDtos.Count} food items, page {searchParams.Page} of {pageCount}");

			// Return paged result
			return new PagedResultDTO<FoodDTO>
			{
				Items = foodDtos,
				TotalCount = totalCount,
				PageCount = pageCount,
				CurrentPage = searchParams.Page
			};
		}

		public async Task<FoodDTO> CreateAsync(FoodCreateDTO foodDto)
		{
			// Check if food with same name already exists
			if (await _context.Foods.AnyAsync(f => f.Name == foodDto.Name))
			{
				await _logService.LogWarningAsync($"Cannot create food: food with name '{foodDto.Name}' already exists");
				return null;
			}

			// Create new food entity
			var food = new Food
			{
				Name = foodDto.Name,
				Description = foodDto.Description,
				Price = foodDto.Price,
				ImageUrl = foodDto.ImageUrl,
				PreparationTime = foodDto.PreparationTime,
				FoodCategoryId = foodDto.FoodCategoryId,
				FoodAllergens = new List<FoodAllergen>()
			};

			// Add allergens if provided
			if (foodDto.AllergenIds != null && foodDto.AllergenIds.Any())
			{
				foreach (var allergenId in foodDto.AllergenIds)
				{
					// Check if allergen exists
					if (!await _context.Allergens.AnyAsync(a => a.Id == allergenId))
					{
						await _logService.LogWarningAsync($"Allergen with id={allergenId} not found");
						continue;
					}

					food.FoodAllergens.Add(new FoodAllergen { FoodId = food.Id, AllergenId = allergenId });
				}
			}

			// Add to database
			_context.Foods.Add(food);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Food item with id={food.Id} was created");

			// Load related data for the response
			await _context.Entry(food)
				.Reference(f => f.FoodCategory)
				.LoadAsync();

			await _context.Entry(food)
				.Collection(f => f.FoodAllergens)
				.Query()
				.Include(fa => fa.Allergen)
				.LoadAsync();

			return MapToFoodDTO(food);
		}

		public async Task<FoodDTO> UpdateAsync(int id, FoodUpdateDTO foodDto)
		{
			// Find the food to update
			var food = await _context.Foods
				.Include(f => f.FoodAllergens)
				.FirstOrDefaultAsync(f => f.Id == id);

			if (food == null)
			{
				await _logService.LogWarningAsync($"Cannot update food: food with id={id} not found");
				return null;
			}

			// Check if name is being changed and if it would conflict
			if (food.Name != foodDto.Name && await _context.Foods.AnyAsync(f => f.Name == foodDto.Name && f.Id != id))
			{
				await _logService.LogWarningAsync($"Cannot update food: food with name '{foodDto.Name}' already exists");
				return null;
			}

			// Update properties
			food.Name = foodDto.Name;
			food.Description = foodDto.Description;
			food.Price = foodDto.Price;
			food.ImageUrl = foodDto.ImageUrl;
			food.PreparationTime = foodDto.PreparationTime;
			food.FoodCategoryId = foodDto.FoodCategoryId;

			// Update allergens if provided
			if (foodDto.AllergenIds != null)
			{
				// Remove existing allergens
				food.FoodAllergens.Clear();

				// Add new allergens
				foreach (var allergenId in foodDto.AllergenIds)
				{
					// Check if allergen exists
					if (!await _context.Allergens.AnyAsync(a => a.Id == allergenId))
					{
						await _logService.LogWarningAsync($"Allergen with id={allergenId} not found");
						continue;
					}

					food.FoodAllergens.Add(new FoodAllergen { FoodId = food.Id, AllergenId = allergenId });
				}
			}

			// Save changes
			try
			{
				await _context.SaveChangesAsync();
				await _logService.LogInformationAsync($"Food item with id={id} was updated");
			}
			catch (Exception ex)
			{
				await _logService.LogErrorAsync($"Error updating food with id={id}: {ex.Message}");
				return null;
			}

			// Load related data for the response
			await _context.Entry(food)
				.Reference(f => f.FoodCategory)
				.LoadAsync();

			await _context.Entry(food)
				.Collection(f => f.FoodAllergens)
				.Query()
				.Include(fa => fa.Allergen)
				.LoadAsync();

			return MapToFoodDTO(food);
		}

		public async Task<bool> DeleteAsync(int id)
		{
			// Find the food to delete
			var food = await _context.Foods.FindAsync(id);
			if (food == null)
			{
				await _logService.LogWarningAsync($"Cannot delete food: food with id={id} not found");
				return false;
			}

			// Check if food is used in any orders
			var isUsedInOrders = await _context.OrderItems.AnyAsync(oi => oi.FoodId == id);
			if (isUsedInOrders)
			{
				await _logService.LogWarningAsync($"Cannot delete food with id={id}: it is used in orders");
				return false;
			}

			// Delete food
			_context.Foods.Remove(food);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Food item with id={id} was deleted");
			return true;
		}

		// Helper method to map Food entity to FoodDTO
		private FoodDTO MapToFoodDTO(Food food)
		{
			return new FoodDTO
			{
				Id = food.Id,
				Name = food.Name,
				Description = food.Description,
				Price = food.Price,
				ImageUrl = food.ImageUrl,
				PreparationTime = food.PreparationTime,
				FoodCategoryId = food.FoodCategoryId,
				FoodCategoryName = food.FoodCategory?.Name,
				Allergens = food.FoodAllergens?.Select(fa => new AllergenDTO
				{
					Id = fa.Allergen.Id,
					Name = fa.Allergen.Name,
					Description = fa.Allergen.Description
				}).ToList() ?? new List<AllergenDTO>()
			};
		}
	}
}