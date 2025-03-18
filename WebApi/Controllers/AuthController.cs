using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebAPI.DTOs;
using WebAPI.Services;

namespace WebAPI.Controllers
{
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