using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using TravelOrganizationWebApp.Models;

namespace TravelOrganizationWebApp.Services
{
    public interface IUnsplashService
    {
        Task<string?> GetRandomImageUrlAsync(string query);
        Task<string?> GetImageUrlAsync(string photoId);
    }

    public class UnsplashService : IUnsplashService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly UnsplashSettings _settings;
        private readonly ILogger<UnsplashService> _logger;

        public UnsplashService(
            HttpClient httpClient,
            IMemoryCache cache,
            UnsplashSettings settings,
            ILogger<UnsplashService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _settings = settings;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_settings.AccessKey}");
        }

        public async Task<string?> GetRandomImageUrlAsync(string query)
        {
            var cacheKey = $"unsplash_random_{query}";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out string? cachedUrl))
            {
                _logger.LogDebug("Retrieved random image URL from cache for query: {Query}", query);
                return cachedUrl;
            }

            try
            {
                var response = await _httpClient.GetAsync(
                    $"https://api.unsplash.com/photos/random?query={Uri.EscapeDataString(query)}&orientation=landscape");

                if (response.IsSuccessStatusCode)
                {
                    var photo = await response.Content.ReadFromJsonAsync<UnsplashPhoto>();
                    if (photo?.Urls?.Regular != null)
                    {
                        // Cache the result
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
                        _cache.Set(cacheKey, photo.Urls.Regular, cacheOptions);

                        // Track download as per Unsplash guidelines
                        await TrackDownloadAsync(photo.Links.DownloadLocation);

                        return photo.Urls.Regular;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting random image for query: {Query}", query);
            }

            return null;
        }

        public async Task<string?> GetImageUrlAsync(string photoId)
        {
            var cacheKey = $"unsplash_photo_{photoId}";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out string? cachedUrl))
            {
                _logger.LogDebug("Retrieved image URL from cache for photo ID: {PhotoId}", photoId);
                return cachedUrl;
            }

            try
            {
                var response = await _httpClient.GetAsync($"https://api.unsplash.com/photos/{photoId}");

                if (response.IsSuccessStatusCode)
                {
                    var photo = await response.Content.ReadFromJsonAsync<UnsplashPhoto>();
                    if (photo?.Urls?.Regular != null)
                    {
                        // Cache the result
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
                        _cache.Set(cacheKey, photo.Urls.Regular, cacheOptions);

                        // Track download as per Unsplash guidelines
                        await TrackDownloadAsync(photo.Links.DownloadLocation);

                        return photo.Urls.Regular;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting image for photo ID: {PhotoId}", photoId);
            }

            return null;
        }

        private async Task TrackDownloadAsync(string downloadLocation)
        {
            try
            {
                await _httpClient.GetAsync(downloadLocation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to track download for location: {Location}", downloadLocation);
            }
        }
    }

    public class UnsplashPhoto
    {
        public UnsplashUrls Urls { get; set; } = new();
        public UnsplashLinks Links { get; set; } = new();
    }

    public class UnsplashUrls
    {
        public string? Regular { get; set; }
    }

    public class UnsplashLinks
    {
        public string DownloadLocation { get; set; } = string.Empty;
    }
} 