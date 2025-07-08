using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
	// Test deployment - API workflow trigger
	/// <summary>
	/// Controller for handling authentication and user account operations
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IJwtService _jwtService;
		private readonly ILogService _logService;

		public AuthController(IUserService userService, IJwtService jwtService, ILogService logService)
		{
			_userService = userService;
			_jwtService = jwtService;
			_logService = logService;
		}

		/// <summary>
		/// Register a new user account
		/// </summary>
		/// <param name="model">Registration information including username, password, email and personal details</param>
		/// <remarks>
		/// This endpoint is publicly accessible - no authentication required
		/// </remarks>
		/// <returns>Confirmation of successful registration</returns>
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDTO model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _userService.RegisterAsync(model);
			if (user == null)
				return BadRequest("Username or email already exists");

			return Ok(new { message = "Registration successful" });
		}

		/// <summary>
		/// Authenticate a user and generate JWT token
		/// </summary>
		/// <param name="model">Login credentials (username and password)</param>
		/// <remarks>
		/// This endpoint is publicly accessible - no authentication required
		/// </remarks>
		/// <returns>JWT token and user information if authentication is successful</returns>
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDTO model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var user = await _userService.AuthenticateAsync(model.Username, model.Password);
			if (user == null)
				return BadRequest("Username or password is incorrect");

			var token = _jwtService.GenerateToken(user);
			var expiryDate = DateTime.UtcNow.AddMinutes(120); // Match with token expiry

			return Ok(new TokenResponseDTO
			{
				Token = token,
				Username = user.Username,
				IsAdmin = user.IsAdmin,
				ExpiresAt = expiryDate.ToString("o")
			});
		}

		/// <summary>
		/// Change the password for the currently authenticated user
		/// </summary>
		/// <param name="model">Password change details including current and new password</param>
		/// <remarks>
		/// This endpoint requires authentication (any user can change their own password)
		/// </remarks>
		/// <returns>Confirmation of successful password change</returns>
		[Authorize]
		[HttpPost("changepassword")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			// Get user ID from token claims
			var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
			if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
				return Unauthorized();

			var result = await _userService.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
			if (!result)
				return BadRequest("Current password is incorrect");

			return Ok(new { message = "Password changed successfully" });
		}
	}
}