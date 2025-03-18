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
	public interface IAllergenService
	{
		Task<List<AllergenDTO>> GetAllAsync();
		Task<AllergenDTO> GetByIdAsync(int id);
		Task<AllergenDTO> CreateAsync(AllergenCreateDTO allergenDto);
		Task<AllergenDTO> UpdateAsync(int id, AllergenUpdateDTO allergenDto);
		Task<bool> DeleteAsync(int id);
	}

	public class AllergenService : IAllergenService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogService _logService;

		public AllergenService(ApplicationDbContext context, ILogService logService)
		{
			_context = context;
			_logService = logService;
		}

		public async Task<List<AllergenDTO>> GetAllAsync()
		{
			var allergens = await _context.Allergens
				.OrderBy(a => a.Name)
				.ToListAsync();

			var allergenDtos = allergens.Select(a => new AllergenDTO
			{
				Id = a.Id,
				Name = a.Name,
				Description = a.Description
			}).ToList();

			await _logService.LogInformationAsync($"Retrieved all {allergenDtos.Count} allergens");
			return allergenDtos;
		}

		public async Task<AllergenDTO> GetByIdAsync(int id)
		{
			var allergen = await _context.Allergens.FindAsync(id);
			if (allergen == null)
			{
				await _logService.LogWarningAsync($"Allergen with id={id} not found");
				return null;
			}

			await _logService.LogInformationAsync($"Retrieved allergen with id={id}");
			return new AllergenDTO
			{
				Id = allergen.Id,
				Name = allergen.Name,
				Description = allergen.Description
			};
		}

		public async Task<AllergenDTO> CreateAsync(AllergenCreateDTO allergenDto)
		{
			// Check if allergen with same name already exists
			if (await _context.Allergens.AnyAsync(a => a.Name == allergenDto.Name))
			{
				await _logService.LogWarningAsync($"Cannot create allergen: allergen with name '{allergenDto.Name}' already exists");
				return null;
			}

			// Create new allergen
			var allergen = new Allergen
			{
				Name = allergenDto.Name,
				Description = allergenDto.Description
			};

			_context.Allergens.Add(allergen);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Allergen with id={allergen.Id} was created");
			return new AllergenDTO
			{
				Id = allergen.Id,
				Name = allergen.Name,
				Description = allergen.Description
			};
		}

		public async Task<AllergenDTO> UpdateAsync(int id, AllergenUpdateDTO allergenDto)
		{
			// Find the allergen to update
			var allergen = await _context.Allergens.FindAsync(id);
			if (allergen == null)
			{
				await _logService.LogWarningAsync($"Cannot update allergen: allergen with id={id} not found");
				return null;
			}

			// Check if name is being changed and if it would conflict
			if (allergen.Name != allergenDto.Name && await _context.Allergens.AnyAsync(a => a.Name == allergenDto.Name && a.Id != id))
			{
				await _logService.LogWarningAsync($"Cannot update allergen: allergen with name '{allergenDto.Name}' already exists");
				return null;
			}

			// Update properties
			allergen.Name = allergenDto.Name;
			allergen.Description = allergenDto.Description;

			// Save changes
			try
			{
				await _context.SaveChangesAsync();
				await _logService.LogInformationAsync($"Allergen with id={id} was updated");
			}
			catch (Exception ex)
			{
				await _logService.LogErrorAsync($"Error updating allergen with id={id}: {ex.Message}");
				return null;
			}

			return new AllergenDTO
			{
				Id = allergen.Id,
				Name = allergen.Name,
				Description = allergen.Description
			};
		}

		public async Task<bool> DeleteAsync(int id)
		{
			// Find the allergen to delete
			var allergen = await _context.Allergens.FindAsync(id);
			if (allergen == null)
			{
				await _logService.LogWarningAsync($"Cannot delete allergen: allergen with id={id} not found");
				return false;
			}

			// Check if allergen is used by any foods
			var isUsedByFoods = await _context.FoodAllergens.AnyAsync(fa => fa.AllergenId == id);
			if (isUsedByFoods)
			{
				await _logService.LogWarningAsync($"Cannot delete allergen with id={id}: it is used by food items");
				return false;
			}

			// Delete allergen
			_context.Allergens.Remove(allergen);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Allergen with id={id} was deleted");
			return true;
		}
	}
}