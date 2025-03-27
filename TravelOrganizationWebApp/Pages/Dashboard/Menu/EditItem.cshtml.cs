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
using Microsoft.AspNetCore.Mvc.Rendering;

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
        
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload categories and allergens on validation error
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                Allergens = (await _foodService.GetAllAllergensAsync()).ToList();
                return Page();
            }
            
            // Handle image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(MenuItem.ImageUrl))
                {
                    DeleteOldImage(MenuItem.ImageUrl);
                }
                
                // Save new image
                MenuItem.ImageUrl = await SaveImageAsync(ImageFile);
            }
            else if (RemoveImage)
            {
                // Delete image without replacement
                if (!string.IsNullOrEmpty(MenuItem.ImageUrl))
                {
                    DeleteOldImage(MenuItem.ImageUrl);
                    MenuItem.ImageUrl = string.Empty;
                }
            }
            
            try
            {
                // Associate selected allergens with the menu item
                MenuItem.Allergens = new List<AllergenModel>();
                
                if (SelectedAllergenIds != null && SelectedAllergenIds.Any())
                {
                    var allAllergens = await _foodService.GetAllAllergensAsync();
                    MenuItem.Allergens = allAllergens
                        .Where(a => SelectedAllergenIds.Contains(a.Id))
                        .ToList();
                }
                
                // Save menu item
                if (MenuItem.Id == 0)
                {
                    // Create mode
                    await _foodService.CreateFoodAsync(MenuItem);
                    TempData["SuccessMessage"] = "Item created successfully.";
                }
                else
                {
                    // Edit mode
                    await _foodService.UpdateFoodAsync(MenuItem);
                    TempData["SuccessMessage"] = "Item updated successfully.";
                }
                
                return RedirectToPage("/Dashboard/Menu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving menu item");
                ModelState.AddModelError(string.Empty, ex.Message);
                
                // Reload categories and allergens on error
                Categories = (await _foodService.GetAllCategoriesAsync()).ToList();
                Allergens = (await _foodService.GetAllAllergensAsync()).ToList();
                SelectedAllergenIds = SelectedAllergenIds.ToList();
                
                return Page();
            }
        }
        
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var item = await _foodService.GetFoodByIdAsync(id);
                
                if (item == null)
                {
                    TempData["ErrorMessage"] = "Item not found.";
                    return RedirectToPage("/Dashboard/Menu");
                }
                
                // Delete associated image
                if (!string.IsNullOrEmpty(item.ImageUrl))
                {
                    DeleteOldImage(item.ImageUrl);
                }
                
                // Delete the item
                await _foodService.DeleteFoodAsync(id);
                
                TempData["SuccessMessage"] = "Item deleted successfully.";
                return RedirectToPage("/Dashboard/Menu");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting item: {ex.Message}";
                return RedirectToPage("/Dashboard/Menu");
            }
        }
        
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            try
            {
                // Generate unique file name
                string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                string uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "MenuItems");
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                // Save file
                string filePath = Path.Combine(uploadPath, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                
                // Return relative URL for the image
                return $"/Images/MenuItems/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading image: {ex.Message}", ex);
            }
        }
        
        private void DeleteOldImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("http"))
                return; // Skip if it's a URL or empty
                
            try
            {
                // Get physical path from URL
                string fileName = Path.GetFileName(imageUrl);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", "MenuItems", fileName);
                
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            catch
            {
                // Ignore errors when deleting files - may be in use or not exist
            }
        }
    }
} 