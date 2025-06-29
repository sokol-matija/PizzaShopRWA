using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Services
{
    public class TripRegistrationService : ITripRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public TripRegistrationService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IEnumerable<TripRegistration>> GetAllRegistrationsAsync()
        {
            return await _context.TripRegistrations
                .Include(tr => tr.User)
                .Include(tr => tr.Trip)
                    .ThenInclude(t => t.Destination)
                .ToListAsync();
        }

        public async Task<TripRegistration> GetRegistrationByIdAsync(int id)
        {
            return await _context.TripRegistrations
                .Include(tr => tr.User)
                .Include(tr => tr.Trip)
                    .ThenInclude(t => t.Destination)
                .FirstOrDefaultAsync(tr => tr.Id == id);
        }

        public async Task<IEnumerable<TripRegistration>> GetRegistrationsByUserAsync(int userId)
        {
            return await _context.TripRegistrations
                .Where(tr => tr.UserId == userId)
                .Include(tr => tr.Trip)
                    .ThenInclude(t => t.Destination)
                .ToListAsync();
        }

        public async Task<IEnumerable<TripRegistration>> GetRegistrationsByTripAsync(int tripId)
        {
            return await _context.TripRegistrations
                .Where(tr => tr.TripId == tripId)
                .Include(tr => tr.User)
                .ToListAsync();
        }

        public async Task<TripRegistration> CreateRegistrationAsync(TripRegistration registration)
        {
            // Check if the trip exists and has available slots
            var trip = await _context.Trips.FindAsync(registration.TripId);
            if (trip == null)
                return null;

            // Count current registrations for this trip
            var currentParticipants = await _context.TripRegistrations
                .Where(tr => tr.TripId == registration.TripId)
                .SumAsync(tr => tr.NumberOfParticipants);

            // Check if there's enough capacity
            if (currentParticipants + registration.NumberOfParticipants > trip.MaxParticipants)
                return null;

            // Set registration date if not provided
            if (registration.RegistrationDate == default)
                registration.RegistrationDate = DateTime.Now;

            // Calculate total price if not set
            if (registration.TotalPrice <= 0)
                registration.TotalPrice = trip.Price * registration.NumberOfParticipants;

            _context.TripRegistrations.Add(registration);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Created registration for trip {trip.Name} by user {registration.UserId}");
            return registration;
        }

        public async Task<TripRegistration> UpdateRegistrationAsync(int id, TripRegistration registration)
        {
            var existingRegistration = await _context.TripRegistrations.FindAsync(id);
            if (existingRegistration == null)
                return null;

            var trip = await _context.Trips.FindAsync(existingRegistration.TripId);
            if (trip == null)
                return null;

            // If number of participants is changing, check capacity
            if (registration.NumberOfParticipants != existingRegistration.NumberOfParticipants)
            {
                var currentParticipants = await _context.TripRegistrations
                    .Where(tr => tr.TripId == existingRegistration.TripId && tr.Id != id)
                    .SumAsync(tr => tr.NumberOfParticipants);

                if (currentParticipants + registration.NumberOfParticipants > trip.MaxParticipants)
                    return null;

                existingRegistration.NumberOfParticipants = registration.NumberOfParticipants;
                existingRegistration.TotalPrice = trip.Price * registration.NumberOfParticipants;
            }

            existingRegistration.Status = registration.Status;

            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Updated registration {id}");
            return existingRegistration;
        }

        public async Task<bool> DeleteRegistrationAsync(int id)
        {
            var registration = await _context.TripRegistrations.FindAsync(id);
            if (registration == null)
                return false;

            _context.TripRegistrations.Remove(registration);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Deleted registration {id}");
            return true;
        }

        public async Task<bool> UpdateRegistrationStatusAsync(int id, string status)
        {
            var registration = await _context.TripRegistrations.FindAsync(id);
            if (registration == null)
                return false;

            registration.Status = status;
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Updated registration {id} status to {status}");
            return true;
        }
    }
} 