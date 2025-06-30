using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using WebApp.Models;
using WebApp.Services;

namespace WebApp.Pages.Trips
{
    /// <summary>
    /// Page model for editing trips (admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            ITripService tripService,
            IDestinationService destinationService,
            ILogger<EditModel> logger)
        {
            _tripService = tripService;
            _destinationService = destinationService;
            _logger = logger;
        }

        [BindProperty]
        public EditTripModel Trip { get; set; } = new EditTripModel();

        public List<SelectListItem> Destinations { get; set; } = new List<SelectListItem>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(id);
                
                if (trip == null)
                {
                    _logger.LogWarning("Trip not found for edit: {Id}", id);
                    return NotFound();
                }

                // Map trip to edit model
                Trip = new EditTripModel
                {
                    Id = trip.Id,
                    Title = trip.Title,
                    Description = trip.Description,
                    StartDate = trip.StartDate,
                    EndDate = trip.EndDate,
                    Price = trip.Price,
                    Capacity = trip.Capacity,
                    ImageUrl = trip.ImageUrl,
                    DestinationId = trip.DestinationId
                };

                await LoadDestinationsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving trip for edit: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the trip.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDestinationsAsync();
                return Page();
            }

            // Validate dates
            if (Trip.StartDate >= Trip.EndDate)
            {
                ModelState.AddModelError("Trip.EndDate", "End date must be after start date.");
                await LoadDestinationsAsync();
                return Page();
            }

            if (Trip.StartDate < DateTime.Today)
            {
                ModelState.AddModelError("Trip.StartDate", "Start date cannot be in the past.");
                await LoadDestinationsAsync();
                return Page();
            }

            try
            {
                var tripModel = new TripModel
                {
                    Id = Trip.Id,
                    Title = Trip.Title,
                    Description = Trip.Description,
                    StartDate = Trip.StartDate,
                    EndDate = Trip.EndDate,
                    Price = Trip.Price,
                    Capacity = Trip.Capacity,
                    ImageUrl = Trip.ImageUrl,
                    DestinationId = Trip.DestinationId
                };

                var result = await _tripService.UpdateTripAsync(Trip.Id, tripModel);
                
                if (result != null)
                {
                    _logger.LogInformation("Trip updated: {Id}", Trip.Id);
                    TempData["SuccessMessage"] = "Trip updated successfully!";
                    return RedirectToPage("./Details", new { id = Trip.Id });
                }
                else
                {
                    _logger.LogWarning("Failed to update trip: {Id}", Trip.Id);
                    ModelState.AddModelError("", "Failed to update trip. Please try again.");
                    await LoadDestinationsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating trip: {Id}", Trip.Id);
                ModelState.AddModelError("", "An error occurred while updating the trip. Please try again.");
                await LoadDestinationsAsync();
                return Page();
            }
        }

        private async Task LoadDestinationsAsync()
        {
            try
            {
                var destinations = await _destinationService.GetAllDestinationsAsync();
                Destinations = destinations
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = $"{d.Name} - {d.City}, {d.Country}"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading destinations");
                Destinations = new List<SelectListItem>();
            }
        }
    }

    /// <summary>
    /// Model for editing a trip
    /// </summary>
    public class EditTripModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price (USD)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500")]
        [Display(Name = "Maximum Participants")]
        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public int DestinationId { get; set; }
    }
} 