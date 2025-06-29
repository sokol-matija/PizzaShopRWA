using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public TripService(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        public async Task<IEnumerable<Trip>> GetAllTripsAsync()
        {
            return await _context.Trips
                .Include(t => t.Destination)
                .Include(t => t.TripGuides)
                    .ThenInclude(tg => tg.Guide)
                .ToListAsync();
        }

        public async Task<Trip> GetTripByIdAsync(int id)
        {
            return await _context.Trips
                .Include(t => t.Destination)
                .Include(t => t.TripGuides)
                    .ThenInclude(tg => tg.Guide)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Trip>> GetTripsByDestinationAsync(int destinationId)
        {
            return await _context.Trips
                .Where(t => t.DestinationId == destinationId)
                .Include(t => t.Destination)
                .Include(t => t.TripGuides)
                    .ThenInclude(tg => tg.Guide)
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> SearchTripsAsync(string? name, string? description, int page, int count)
        {
            // Log the search request
            await _logService.LogInformationAsync($"Searching trips with name: '{name}', description: '{description}', page: {page}, count: {count}");

            // Start with base query
            var query = _context.Trips
                .Include(t => t.Destination)
                .Include(t => t.TripGuides)
                    .ThenInclude(tg => tg.Guide)
                .Include(t => t.TripRegistrations)
                .AsQueryable();

            // Apply name filter if provided
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(t => t.Name.Contains(name));
            }

            // Apply description filter if provided
            if (!string.IsNullOrWhiteSpace(description))
            {
                query = query.Where(t => t.Description != null && t.Description.Contains(description));
            }

            // Apply pagination
            var results = await query
                .Skip((page - 1) * count)
                .Take(count)
                .ToListAsync();

            // Log the results
            await _logService.LogInformationAsync($"Search returned {results.Count} trips for page {page}");

            return results;
        }

        public async Task<Trip> CreateTripAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Created trip: {trip.Name}");
            return trip;
        }

        public async Task<Trip> UpdateTripAsync(int id, Trip trip)
        {
            var existingTrip = await _context.Trips.FindAsync(id);
            if (existingTrip == null)
                return null;

            existingTrip.Name = trip.Name;
            existingTrip.Description = trip.Description;
            existingTrip.StartDate = trip.StartDate;
            existingTrip.EndDate = trip.EndDate;
            existingTrip.Price = trip.Price;
            existingTrip.ImageUrl = trip.ImageUrl;
            existingTrip.MaxParticipants = trip.MaxParticipants;
            existingTrip.DestinationId = trip.DestinationId;

            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Updated trip: {trip.Name}");
            return existingTrip;
        }

        public async Task<bool> DeleteTripAsync(int id)
        {
            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
                return false;

            // Check if there are any registrations for this trip
            bool hasRegistrations = await _context.TripRegistrations.AnyAsync(tr => tr.TripId == id);
            if (hasRegistrations)
                return false;

            // Remove any guide assignments
            var tripGuides = await _context.TripGuides.Where(tg => tg.TripId == id).ToListAsync();
            _context.TripGuides.RemoveRange(tripGuides);

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Deleted trip: {trip.Name}");
            return true;
        }

        public async Task<bool> AssignGuideToTripAsync(int tripId, int guideId)
        {
            // Check if both trip and guide exist
            var trip = await _context.Trips.FindAsync(tripId);
            var guide = await _context.Guides.FindAsync(guideId);
            if (trip == null || guide == null)
                return false;

            // Check if the assignment already exists
            bool alreadyAssigned = await _context.TripGuides.AnyAsync(tg => tg.TripId == tripId && tg.GuideId == guideId);
            if (alreadyAssigned)
                return true;

            // Create the assignment
            var tripGuide = new TripGuide
            {
                TripId = tripId,
                GuideId = guideId
            };

            _context.TripGuides.Add(tripGuide);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Assigned guide {guide.Name} to trip {trip.Name}");
            return true;
        }

        public async Task<bool> RemoveGuideFromTripAsync(int tripId, int guideId)
        {
            var tripGuide = await _context.TripGuides
                .FirstOrDefaultAsync(tg => tg.TripId == tripId && tg.GuideId == guideId);
            if (tripGuide == null)
                return false;

            _context.TripGuides.Remove(tripGuide);
            await _context.SaveChangesAsync();
            await _logService.LogInformationAsync($"Removed guide from trip {tripId}");
            return true;
        }
    }
} 