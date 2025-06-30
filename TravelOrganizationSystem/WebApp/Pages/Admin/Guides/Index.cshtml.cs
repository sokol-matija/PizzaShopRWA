using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Admin.Guides
{
    /// <summary>
    /// Admin page for managing travel guides
    /// Provides CRUD operations and AJAX functionality
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly IGuideService _guideService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IGuideService guideService, ILogger<IndexModel> logger)
        {
            _guideService = guideService;
            _logger = logger;
        }

        /// <summary>
        /// List of guides to display
        /// </summary>
        public IEnumerable<GuideModel> Guides { get; set; } = new List<GuideModel>();

        /// <summary>
        /// Error message to display if something goes wrong
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Search filter for guides
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? SearchFilter { get; set; }

        /// <summary>
        /// Load guides page with optional search filtering
        /// </summary>
        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Loading admin guides page with search filter: {SearchFilter}", SearchFilter ?? "none");
                
                // Get all guides from the service
                var allGuides = await _guideService.GetAllGuidesAsync();
                
                // Apply search filter if provided
                if (!string.IsNullOrWhiteSpace(SearchFilter))
                {
                    var searchTerm = SearchFilter.ToLowerInvariant();
                    Guides = allGuides.Where(g => 
                        (!string.IsNullOrEmpty(g.FirstName) && g.FirstName.ToLowerInvariant().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(g.LastName) && g.LastName.ToLowerInvariant().Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(g.Email) && g.Email.ToLowerInvariant().Contains(searchTerm)) ||
                        g.FullName.ToLowerInvariant().Contains(searchTerm)
                    ).ToList();
                    
                    _logger.LogInformation("Applied search filter '{SearchFilter}', found {FilteredCount} out of {TotalCount} guides", 
                        SearchFilter, Guides.Count(), allGuides.Count());
                }
                else
                {
                    Guides = allGuides;
                    _logger.LogInformation("Loaded {Count} guides without filter", Guides.Count());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading guides for admin page");
                ErrorMessage = "Unable to load guides. Please try again later.";
                Guides = new List<GuideModel>();
            }
        }

        /// <summary>
        /// Handle guide deletion via AJAX POST
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Admin attempting to delete guide with ID: {GuideId}", id);
                
                // First check if guide exists
                var guide = await _guideService.GetGuideByIdAsync(id);
                if (guide == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent guide: {GuideId}", id);
                    return new JsonResult(new { success = false, message = "Guide not found." });
                }
                
                // Attempt to delete the guide
                var success = await _guideService.DeleteGuideAsync(id);
                
                if (success)
                {
                    _logger.LogInformation("Successfully deleted guide: {GuideName} (ID: {GuideId})", guide.FullName, id);
                    TempData["SuccessMessage"] = $"Guide '{guide.FullName}' has been deleted successfully.";
                    
                    return new JsonResult(new { success = true, message = $"Guide '{guide.FullName}' deleted successfully." });
                }
                else
                {
                    _logger.LogWarning("Failed to delete guide: {GuideName} (ID: {GuideId})", guide.FullName, id);
                    return new JsonResult(new { success = false, message = "Failed to delete guide. The guide may be assigned to active trips." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting guide with ID: {GuideId}", id);
                return new JsonResult(new { success = false, message = "An error occurred while deleting the guide." });
            }
        }

        /// <summary>
        /// Handle AJAX search request
        /// Returns filtered guides as JSON
        /// </summary>
        public async Task<IActionResult> OnGetSearchAsync(string searchTerm = "")
        {
            try
            {
                _logger.LogInformation("AJAX search request for guides with term: '{SearchTerm}'", searchTerm);
                
                var allGuides = await _guideService.GetAllGuidesAsync();
                
                IEnumerable<GuideModel> filteredGuides;
                
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLowerInvariant();
                    filteredGuides = allGuides.Where(g => 
                        (!string.IsNullOrEmpty(g.FirstName) && g.FirstName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(g.LastName) && g.LastName.ToLowerInvariant().Contains(searchLower)) ||
                        (!string.IsNullOrEmpty(g.Email) && g.Email.ToLowerInvariant().Contains(searchLower)) ||
                        g.FullName.ToLowerInvariant().Contains(searchLower)
                    );
                }
                else
                {
                    filteredGuides = allGuides;
                }
                
                // Return simplified guide data for AJAX response
                var guideData = filteredGuides.Select(g => new
                {
                    id = g.Id,
                    fullName = g.FullName,
                    email = g.Email,
                    phoneNumber = g.PhoneNumber,
                    bio = g.Bio,
                    photoUrl = g.PhotoUrl,
                    yearsExperience = g.YearsExperience,
                    hasPhoto = !string.IsNullOrEmpty(g.PhotoUrl)
                });
                
                _logger.LogInformation("AJAX search returned {Count} guides for term: '{SearchTerm}'", 
                    guideData.Count(), searchTerm);
                
                return new JsonResult(new { success = true, guides = guideData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during AJAX guide search for term: '{SearchTerm}'", searchTerm);
                return new JsonResult(new { success = false, message = "Search failed. Please try again." });
            }
        }
    }
} 