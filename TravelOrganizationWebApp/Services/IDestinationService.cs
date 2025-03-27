using System.Text.Json;
using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    public interface IDestinationService
    {
        Task<List<Destination>> GetAllDestinationsAsync();
        Task<Destination?> GetDestinationByIdAsync(int id);
        Task<bool> CreateDestinationAsync(Destination destination);
        Task<bool> UpdateDestinationAsync(Destination destination);
        Task<bool> DeleteDestinationAsync(int id);
    }

    public class DestinationService : IDestinationService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<DestinationService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public DestinationService(IApiService apiService, ILogger<DestinationService> logger)
        {
            _apiService = apiService;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                MaxDepth = 64
            };
        }

        public async Task<List<Destination>> GetAllDestinationsAsync()
        {
            try
            {
                var response = await _apiService.GetAsync("Destination");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Destination>>(content, _jsonOptions) ?? new List<Destination>();
                }
                
                _logger.LogError($"Failed to get destinations: {response.StatusCode}");
                return new List<Destination>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching destinations");
                return new List<Destination>();
            }
        }

        public async Task<Destination?> GetDestinationByIdAsync(int id)
        {
            try
            {
                var response = await _apiService.GetAsync($"Destination/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Destination>(content, _jsonOptions);
                }
                
                _logger.LogError($"Failed to get destination with ID {id}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching destination with ID {id}");
                return null;
            }
        }

        public async Task<bool> CreateDestinationAsync(Destination destination)
        {
            try
            {
                var response = await _apiService.PostAsync("Destination", destination);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating destination");
                return false;
            }
        }

        public async Task<bool> UpdateDestinationAsync(Destination destination)
        {
            try
            {
                var response = await _apiService.PutAsync($"Destination/{destination.Id}", destination);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating destination with ID {destination.Id}");
                return false;
            }
        }

        public async Task<bool> DeleteDestinationAsync(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"Destination/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting destination with ID {id}");
                return false;
            }
        }
    }
} 