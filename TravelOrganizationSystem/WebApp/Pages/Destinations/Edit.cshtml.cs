using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Destinations
{
    /// <summary>
    /// Page model for editing destinations (admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly IDestinationService _destinationService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            IDestinationService destinationService,
            ILogger<EditModel> logger)
        {
            _destinationService = destinationService;
            _logger = logger;
        }

        [BindProperty]
        public DestinationModel Destination { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var destination = await _destinationService.GetDestinationByIdAsync(id);
                
                if (destination == null)
                {
                    _logger.LogWarning("Destination not found for edit: {Id}", id);
                    return NotFound();
                }

                Destination = destination;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving destination for edit: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the destination.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                await _destinationService.UpdateDestinationAsync(Destination.Id, Destination);
                
                _logger.LogInformation("Destination updated: {Id}", Destination.Id);
                TempData["SuccessMessage"] = "Destination updated successfully!";
                
                return RedirectToPage("./Details", new { id = Destination.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating destination: {Id}", Destination.Id);
                ModelState.AddModelError("", "An error occurred while updating the destination. Please try again.");
                return Page();
            }
        }
    }
} 