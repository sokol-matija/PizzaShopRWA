using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAPI.Services;
using WebAPI.DTOs;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Controller for user management operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogService _logService;

        public UserController(IUserService userService, ILogService logService)
        {
            _userService = userService;
            _logService = logService;
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">The user ID to retrieve</param>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>User details without sensitive information</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            
            if (user == null)
                return NotFound();

            // Return user data without sensitive information
            return Ok(new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsAdmin = user.IsAdmin
            });
        }

        /// <summary>
        /// Get the currently authenticated user's information
        /// </summary>
        /// <remarks>
        /// This endpoint requires authentication (any authenticated user can access their own information)
        /// </remarks>
        /// <returns>Current user details without sensitive information</returns>
        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Extract user ID from the token claims
            var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            
            if (user == null)
                return NotFound();

            // Return user data without sensitive information
            return Ok(new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsAdmin = user.IsAdmin
            });
        }

        /// <summary>
        /// Update the currently authenticated user's profile information
        /// </summary>
        /// <param name="model">Updated profile information</param>
        /// <remarks>
        /// This endpoint requires authentication (any authenticated user can update their own profile)
        /// Users cannot change their Username or IsAdmin status through this endpoint
        /// </remarks>
        /// <returns>Updated user details without sensitive information</returns>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Extract user ID from the token claims
            var userIdClaim = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Update the allowed profile fields
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var updatedUser = await _userService.UpdateProfileAsync(user);
            if (updatedUser == null)
                return BadRequest("Failed to update profile");

            // Log the profile update
            await _logService.LogInformationAsync($"User {user.Username} updated their profile");

            // Return updated user data without sensitive information
            return Ok(new UserDTO
            {
                Id = updatedUser.Id,
                Username = updatedUser.Username,
                Email = updatedUser.Email,
                FirstName = updatedUser.FirstName,
                LastName = updatedUser.LastName,
                PhoneNumber = updatedUser.PhoneNumber,
                Address = updatedUser.Address,
                IsAdmin = updatedUser.IsAdmin
            });
        }

        /// <summary>
        /// Get a list of all users in the system
        /// </summary>
        /// <remarks>
        /// This endpoint requires Admin role access
        /// </remarks>
        /// <returns>List of all users without sensitive information</returns>
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            
            var userDtos = users.Select(user => new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsAdmin = user.IsAdmin
            }).ToList();
            
            return Ok(userDtos);
        }
    }
} 