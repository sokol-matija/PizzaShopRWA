using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Services
{
    public interface ITripRegistrationService
    {
        Task<IEnumerable<TripRegistration>> GetAllRegistrationsAsync();
        Task<TripRegistration> GetRegistrationByIdAsync(int id);
        Task<IEnumerable<TripRegistration>> GetRegistrationsByUserAsync(int userId);
        Task<IEnumerable<TripRegistration>> GetRegistrationsByTripAsync(int tripId);
        Task<TripRegistration> CreateRegistrationAsync(TripRegistration registration);
        Task<TripRegistration> UpdateRegistrationAsync(int id, TripRegistration registration);
        Task<bool> DeleteRegistrationAsync(int id);
        Task<bool> UpdateRegistrationStatusAsync(int id, string status);
    }
} 