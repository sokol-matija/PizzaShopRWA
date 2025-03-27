using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Services
{
    public interface ITripService
    {
        Task<IEnumerable<Trip>> GetAllTripsAsync();
        Task<Trip> GetTripByIdAsync(int id);
        Task<IEnumerable<Trip>> GetTripsByDestinationAsync(int destinationId);
        Task<Trip> CreateTripAsync(Trip trip);
        Task<Trip> UpdateTripAsync(int id, Trip trip);
        Task<bool> DeleteTripAsync(int id);
        Task<bool> AssignGuideToTripAsync(int tripId, int guideId);
        Task<bool> RemoveGuideFromTripAsync(int tripId, int guideId);
    }
} 