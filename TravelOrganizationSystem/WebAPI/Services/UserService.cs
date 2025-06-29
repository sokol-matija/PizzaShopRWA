using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Models;
using System.Collections.Generic;

namespace WebAPI.Services
{
	public interface IUserService
	{
		Task<User> AuthenticateAsync(string username, string password);
		Task<User> RegisterAsync(RegisterDTO model);
		Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
		Task<User> GetByIdAsync(int userId);
		Task<List<User>> GetAllUsersAsync();
		Task<User> UpdateProfileAsync(User user);
	}

	public class UserService : IUserService
	{
		private readonly ApplicationDbContext _context;
		private readonly ILogService _logService;

		public UserService(ApplicationDbContext context, ILogService logService)
		{
			_context = context;
			_logService = logService;
		}

		public async Task<User> AuthenticateAsync(string username, string password)
		{
			if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
				return null;

			var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);

			// Check if user exists
			if (user == null)
			{
				await _logService.LogWarningAsync($"Authentication failed: user '{username}' not found");
				return null;
			}

			// Check if password is correct
			if (!VerifyPasswordHash(password, user.PasswordHash))
			{
				await _logService.LogWarningAsync($"Authentication failed: invalid password for user '{username}'");
				return null;
			}

			await _logService.LogInformationAsync($"User '{username}' successfully authenticated");
			return user;
		}

		public async Task<User> RegisterAsync(RegisterDTO model)
		{
			// Check if username is already taken
			if (await _context.Users.AnyAsync(u => u.Username == model.Username))
			{
				await _logService.LogWarningAsync($"Registration failed: username '{model.Username}' already exists");
				return null;
			}

			// Check if email is already taken
			if (await _context.Users.AnyAsync(u => u.Email == model.Email))
			{
				await _logService.LogWarningAsync($"Registration failed: email '{model.Email}' already exists");
				return null;
			}

			// Create new user
			var user = new User
			{
				Username = model.Username,
				Email = model.Email,
				PasswordHash = HashPassword(model.Password),
				FirstName = model.FirstName,
				LastName = model.LastName,
				PhoneNumber = model.PhoneNumber,
				Address = model.Address,
				IsAdmin = false // New users are not admins by default
			};

			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"User '{model.Username}' successfully registered");
			return user;
		}

		public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
		{
			var user = await _context.Users.FindAsync(userId);
			if (user == null)
			{
				await _logService.LogWarningAsync($"Password change failed: user with id={userId} not found");
				return false;
			}

			// Verify current password
			if (!VerifyPasswordHash(currentPassword, user.PasswordHash))
			{
				await _logService.LogWarningAsync($"Password change failed: invalid current password for user '{user.Username}'");
				return false;
			}

			// Update password
			user.PasswordHash = HashPassword(newPassword);
			await _context.SaveChangesAsync();

			await _logService.LogInformationAsync($"Password successfully changed for user '{user.Username}'");
			return true;
		}

		public async Task<User> GetByIdAsync(int userId)
		{
			return await _context.Users.FindAsync(userId);
		}

		public async Task<List<User>> GetAllUsersAsync()
		{
			return await _context.Users.ToListAsync();
		}

		public async Task<User> UpdateProfileAsync(User user)
		{
			try
			{
				// Check if email is already taken by another user
				var existingUser = await _context.Users
					.FirstOrDefaultAsync(u => u.Email == user.Email && u.Id != user.Id);
				
				if (existingUser != null)
				{
					await _logService.LogWarningAsync($"Profile update failed: email '{user.Email}' already exists for another user");
					return null;
				}

				// Update the user in the database
				_context.Users.Update(user);
				await _context.SaveChangesAsync();

				await _logService.LogInformationAsync($"Profile successfully updated for user '{user.Username}'");
				return user;
			}
			catch (Exception ex)
			{
				await _logService.LogErrorAsync($"Error updating profile for user '{user.Username}': {ex.Message}");
				return null;
			}
		}

		// Private helper methods for password hashing
		private string HashPassword(string password)
		{
			// In a real application, you would use a proper password hasher like BCrypt
			// For simplicity, we're using ASP.NET Core's PasswordHasher here
			var hasher = new PasswordHasher<User>();
			return hasher.HashPassword(null, password);
		}

		private bool VerifyPasswordHash(string password, string storedHash)
		{
			var hasher = new PasswordHasher<User>();
			var result = hasher.VerifyHashedPassword(null, storedHash, password);
			return result == PasswordVerificationResult.Success;
		}
	}
}