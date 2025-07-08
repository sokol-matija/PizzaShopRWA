using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Destinations
{
    /// <summary>
    /// Page model for viewing destination details
    /// </summary>
    public class DetailsModel : PageModel
    {
        private readonly IDestinationService _destinationService;
        private readonly ITripService _tripService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IDestinationService destinationService,
            ITripService tripService,
            ILogger<DetailsModel> logger)
        {
            _destinationService = destinationService;
            _tripService = tripService;
            _logger = logger;
        }

        public DestinationModel Destination { get; set; } = default!;
        public int TripsCount { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var destination = await _destinationService.GetDestinationByIdAsync(id);
                
                if (destination == null)
                {
                    _logger.LogWarning("Destination not found: {Id}", id);
                    return NotFound();
                }

                Destination = destination;

                // Get trips count for this destination
                var trips = await _tripService.GetTripsByDestinationAsync(id);
                TripsCount = trips?.Count() ?? 0;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving destination: {Id}", id);
                ErrorMessage = "An error occurred while retrieving the destination details.";
                return RedirectToPage("./Index");
            }
        }

        /// <summary>
        /// Handler for deleting a destination (admin only)
        /// </summary>
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                await _destinationService.DeleteDestinationAsync(id);
                _logger.LogInformation("Destination deleted: {Id}", id);
                TempData["SuccessMessage"] = "Destination successfully deleted.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting destination: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the destination.";
                return RedirectToPage("./Index");
            }
        }

        /// <summary>
        /// Handler for updating a destination's image (admin only)
        /// </summary>
        public async Task<IActionResult> OnPostUpdateImageAsync(int id, string imageUrl)
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                ModelState.AddModelError("", "Image URL is required.");
                return await OnGetAsync(id);
            }

            try
            {
                // First get the destination
                var destination = await _destinationService.GetDestinationByIdAsync(id);
                
                if (destination == null)
                {
                    return NotFound();
                }

                // Update the image URL
                destination.ImageUrl = imageUrl;
                
                // Save changes
                await _destinationService.UpdateDestinationAsync(id, destination);
                
                _logger.LogInformation("Destination image updated: {Id}", id);
                TempData["SuccessMessage"] = "Destination image successfully updated.";
                return RedirectToPage("./Details", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating destination image: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the destination image.";
                return await OnGetAsync(id);
            }
        }
    }
} 