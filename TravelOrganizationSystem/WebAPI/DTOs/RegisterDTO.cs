using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
	public class RegisterDTO
	{
		[Required]
		[StringLength(100, MinimumLength = 3)]
		public string Username { get; set; }

		[Required]
		[StringLength(100)]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; }

		[StringLength(100)]
		public string FirstName { get; set; }

		[StringLength(100)]
		public string LastName { get; set; }

		[StringLength(20)]
		[Phone]
		public string PhoneNumber { get; set; }

		[StringLength(200)]
		public string Address { get; set; }
	}

	public class LoginDTO
	{
		[Required]
		public string Username { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }
	}

	public class ChangePasswordDTO
	{
		[Required]
		[DataType(DataType.Password)]
		public string CurrentPassword { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		[DataType(DataType.Password)]
		public string ConfirmNewPassword { get; set; }
	}

	public class TokenResponseDTO
	{
		public string Token { get; set; }
		public string Username { get; set; }
		public bool IsAdmin { get; set; }
		public string ExpiresAt { get; set; }
	}
}