using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Services
{
    public interface IDestinationService
    {
        Task<IEnumerable<Destination>> GetAllDestinationsAsync();
        Task<Destination> GetDestinationByIdAsync(int id);
        Task<Destination> CreateDestinationAsync(Destination destination);
        Task<Destination> UpdateDestinationAsync(int id, Destination destination);
        Task<bool> DeleteDestinationAsync(int id);
    }
} 