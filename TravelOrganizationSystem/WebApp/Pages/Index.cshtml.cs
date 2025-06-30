using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDestinationService _destinationService;

    public IndexModel(ILogger<IndexModel> logger, IDestinationService destinationService)
    {
        _logger = logger;
        _destinationService = destinationService;
    }

    /// <summary>
    /// Featured destinations to display on the home page (first 3 destinations)
    /// </summary>
    public List<DestinationModel> FeaturedDestinations { get; set; } = new List<DestinationModel>();

    /// <summary>
    /// Error message if loading destinations fails
    /// </summary>
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading featured destinations for home page");
            
            // Get all destinations and take the first 3 for featured section
            var allDestinations = await _destinationService.GetAllDestinationsAsync();
            FeaturedDestinations = allDestinations.Take(3).ToList();
            
            _logger.LogInformation("Loaded {Count} featured destinations", FeaturedDestinations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading featured destinations");
            ErrorMessage = "Unable to load featured destinations at this time.";
            FeaturedDestinations = new List<DestinationModel>();
        }
    }
}
