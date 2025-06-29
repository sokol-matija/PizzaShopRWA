using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Services
{
    public class GuideService : IGuideService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public GuideService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IEnumerable<Guide>> GetAllGuidesAsync()
        {
            return await _context.Guides.ToListAsync();
        }

        public async Task<Guide> GetGuideByIdAsync(int id)
        {
            return await _context.Guides.FindAsync(id);
        }

        public async Task<IEnumerable<Guide>> GetGuidesByTripAsync(int tripId)
        {
            return await _context.TripGuides
                .Where(tg => tg.TripId == tripId)
                .Select(tg => tg.Guide)
                .ToListAsync();
        }

        public async Task<Guide> CreateGuideAsync(Guide guide)
        {
            _context.Guides.Add(guide);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Created guide: {guide.Name}");
            return guide;
        }

        public async Task<Guide> UpdateGuideAsync(int id, Guide guide)
        {
            var existingGuide = await _context.Guides.FindAsync(id);
            if (existingGuide == null)
                return null;

            existingGuide.Name = guide.Name;
            existingGuide.Bio = guide.Bio;
            existingGuide.Email = guide.Email;
            existingGuide.Phone = guide.Phone;
            existingGuide.ImageUrl = guide.ImageUrl;
            existingGuide.YearsOfExperience = guide.YearsOfExperience;

            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Updated guide: {guide.Name}");
            return existingGuide;
        }

        public async Task<bool> DeleteGuideAsync(int id)
        {
            var guide = await _context.Guides.FindAsync(id);
            if (guide == null)
                return false;

            // Remove any trip assignments
            var tripGuides = await _context.TripGuides.Where(tg => tg.GuideId == id).ToListAsync();
            _context.TripGuides.RemoveRange(tripGuides);

            _context.Guides.Remove(guide);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Deleted guide: {guide.Name}");
            return true;
        }
    }
} 