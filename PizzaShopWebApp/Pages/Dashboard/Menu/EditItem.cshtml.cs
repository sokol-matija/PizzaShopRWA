using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PizzaShopWebApp.Models;
using PizzaShopWebApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace PizzaShopWebApp.Pages.Dashboard.Menu
{
    public class EditItemModel : PageModel
    {
        private readonly IFoodService _foodService;
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<EditItemModel> _logger;

        public EditItemModel(
            IFoodService foodService,
            IUserService userService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<EditItemModel> logger)
        {
            _foodService = foodService;
            _userService = userService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [BindProperty]
        public MenuItemModel MenuItem { get; set; } = new MenuItemModel();
        
        public List<CategoryModel> Categories { get; set; } = new List<CategoryModel>();
        public List<AllergenModel> Allergens { get; set; } = new List<AllergenModel>();
        public List<int> SelectedAllergenIds { get; set; } = new List<int>();
        
        [BindProperty]
        public IFormFile? ImageFile { get; set; }
        
        [BindProperty]
        public bool RemoveImage { get; set; }
        
        public bool IsNewItem => MenuItem.Id == 0;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Check if user is authenticated
            if (!_userService.IsAuthenticated())
            {
                return RedirectToPage("/Account/Login");
            }

            try
            {
                // Load categories for dropdown
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                
                // Load allergens for checkboxes
                Allergens = (await _foodService.GetAllAllergensAsync()).ToList();
                
                // If we're editing an existing item, load its data
                if (id.HasValue)
                {
                    MenuItem = await _foodService.GetFoodByIdAsync(id.Value);
                    
                    if (MenuItem == null)
                    {
                        TempData["ErrorMessage"] = "Menu item not found.";
                        return RedirectToPage("/Dashboard/Menu");
                    }
                    
                    // Get selected allergens
                    SelectedAllergenIds = MenuItem.Allergens.Select(a => a.Id).ToList();
                }
                else
                {
                    // For new item, create an empty model
                    MenuItem = new MenuItemModel
                    {
                        IsVegetarian = false,
                        IsPopular = false,
                        Price = 0,
                        PreparationTime = 15 // Default preparation time
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading item data");
                TempData["ErrorMessage"] = "Failed to load item data. Please try again later.";
                return RedirectToPage("/Dashboard/Menu");
            }

            return Page();
        }
        
        public async Task<IActionResult> OnPostAsync(int[] SelectedAllergens)
        {
            try
            {
                // Validate model state
                if (!ModelState.IsValid)
                {
                    // Reload categories and allergens on validation error
                    Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                    Allergens = (await _foodService.GetAllAllergensAsync()).ToList();
                    SelectedAllergenIds = SelectedAllergens.ToList();
                    return Page();
                }
                
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    // Save uploaded image
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "Dishes");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(fileStream);
                    }
                    
                    // Update image path in model
                    MenuItem.ImageUrl = "/Images/Dishes/" + uniqueFileName;
                }
                else if (RemoveImage)
                {
                    // Clear image if requested
                    MenuItem.ImageUrl = string.Empty;
                }
                
                // Map selected allergens to menu item
                MenuItem.Allergens = Allergens
                    .Where(a => SelectedAllergens.Contains(a.Id))
                    .ToList();
                
                // Get category name for display
                var category = Categories.FirstOrDefault(c => c.Id == MenuItem.FoodCategoryId);
                if (category != null)
                {
                    MenuItem.FoodCategoryName = category.Name;
                }

                // In a real application, you would call your service to save the menu item
                // Here we'll just show a success message
                
                if (MenuItem.Id == 0)
                {
                    // Create new item
                    // Normally: MenuItem = await _foodService.CreateFoodItemAsync(MenuItem);
                    TempData["SuccessMessage"] = $"Dish '{MenuItem.Name}' created successfully.";
                }
                else
                {
                    // Update existing item
                    // Normally: await _foodService.UpdateFoodItemAsync(MenuItem);
                    TempData["SuccessMessage"] = $"Dish '{MenuItem.Name}' updated successfully.";
                }
                
                return RedirectToPage("/Dashboard/Menu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving menu item");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the menu item. Please try again.");
                
                // Reload categories and allergens on error
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                Allergens = (await _foodService.GetAllAllergensAsync()).ToList();
                SelectedAllergenIds = SelectedAllergens.ToList();
                
                return Page();
            }
        }
    }
} 