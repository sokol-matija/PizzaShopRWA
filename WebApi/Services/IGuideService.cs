using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Services
{
    public interface IGuideService
    {
        Task<IEnumerable<Guide>> GetAllGuidesAsync();
        Task<Guide> GetGuideByIdAsync(int id);
        Task<IEnumerable<Guide>> GetGuidesByTripAsync(int tripId);
        Task<Guide> CreateGuideAsync(Guide guide);
        Task<Guide> UpdateGuideAsync(int id, Guide guide);
        Task<bool> DeleteGuideAsync(int id);
    }
} 