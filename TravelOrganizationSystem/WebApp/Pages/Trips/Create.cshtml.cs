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
    /// Page model for creating new trips
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly IDestinationService _destinationService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(
            ITripService tripService,
            IDestinationService destinationService,
            ILogger<CreateModel> logger)
        {
            _tripService = tripService;
            _destinationService = destinationService;
            _logger = logger;
        }

        [BindProperty]
        public CreateTripModel Trip { get; set; } = new CreateTripModel();

        public List<SelectListItem> Destinations { get; set; } = new List<SelectListItem>();

        public async Task OnGetAsync()
        {
            await LoadDestinationsAsync();
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
                var trip = new TripModel
                {
                    Title = Trip.Title,
                    Description = Trip.Description,
                    StartDate = Trip.StartDate,
                    EndDate = Trip.EndDate,
                    Price = Trip.Price,
                    Capacity = Trip.Capacity,
                    ImageUrl = Trip.ImageUrl,
                    DestinationId = Trip.DestinationId
                };

                var result = await _tripService.CreateTripAsync(trip);
                if (result != null)
                {
                    _logger.LogInformation("Successfully created trip: {Title}", trip.Title);
                    TempData["SuccessMessage"] = "Trip created successfully!";
                    return RedirectToPage("./Index");
                }
                else
                {
                    _logger.LogWarning("Failed to create trip: {Title}", trip.Title);
                    ModelState.AddModelError("", "Failed to create trip. Please try again.");
                    await LoadDestinationsAsync();
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trip: {Title}", Trip.Title);
                ModelState.AddModelError("", "An error occurred while creating the trip. Please try again.");
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
    /// Model for creating a new trip
    /// </summary>
    public class CreateTripModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Price (USD)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 500, ErrorMessage = "Capacity must be between 1 and 500")]
        [Display(Name = "Maximum Participants")]
        public int Capacity { get; set; } = 10;

        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot be longer than 500 characters")]
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Destination is required")]
        [Display(Name = "Destination")]
        public int DestinationId { get; set; }
    }
} 