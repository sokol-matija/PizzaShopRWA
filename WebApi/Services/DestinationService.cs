using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Services
{
    public class DestinationService : IDestinationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public DestinationService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IEnumerable<Destination>> GetAllDestinationsAsync()
        {
            return await _context.Destinations.ToListAsync();
        }

        public async Task<Destination> GetDestinationByIdAsync(int id)
        {
            return await _context.Destinations.FindAsync(id);
        }

        public async Task<Destination> CreateDestinationAsync(Destination destination)
        {
            _context.Destinations.Add(destination);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Created destination: {destination.Name}");
            return destination;
        }

        public async Task<Destination> UpdateDestinationAsync(int id, Destination destination)
        {
            var existingDestination = await _context.Destinations.FindAsync(id);
            if (existingDestination == null)
                return null;

            existingDestination.Name = destination.Name;
            existingDestination.Description = destination.Description;
            existingDestination.Country = destination.Country;
            existingDestination.City = destination.City;
            existingDestination.ImageUrl = destination.ImageUrl;

            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Updated destination: {destination.Name}");
            return existingDestination;
        }

        public async Task<bool> DeleteDestinationAsync(int id)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null)
                return false;

            _context.Destinations.Remove(destination);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Deleted destination: {destination.Name}");
            return true;
        }

        public async Task<bool> UpdateImageUrlAsync(int id, string imageUrl)
        {
            var destination = await _context.Destinations.FindAsync(id);
            if (destination == null)
                return false;

            destination.ImageUrl = imageUrl;
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Updated image URL for destination: {destination.Name}");
            return true;
        }
    }
}