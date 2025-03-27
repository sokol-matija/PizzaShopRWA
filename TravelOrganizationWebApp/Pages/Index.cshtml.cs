using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PizzaShopWebApp.Pages
{
	public class IndexModel : PageModel
	{
		private readonly ILogger<IndexModel> _logger;

		public IndexModel(ILogger<IndexModel> logger)
		{
			_logger = logger;
		}

		public IActionResult OnGet()
		{
			// Check if user is authenticated
			var authToken = HttpContext.Session.GetString("AuthToken");
			
			if (string.IsNullOrEmpty(authToken))
			{
				_logger.LogInformation("User not authenticated, redirecting to login");
				return RedirectToPage("/Account/Login");
			}
			
			// Set the title for the page
			ViewData["Title"] = "Dashboard";
			
			// User is authenticated, show the dashboard
			return Page();
		}
	}
}
