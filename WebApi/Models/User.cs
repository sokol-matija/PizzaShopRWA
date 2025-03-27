using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
	public class User
	{
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Username { get; set; }

		[Required]
		[StringLength(100)]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[StringLength(500)]
		public string PasswordHash { get; set; }

		[StringLength(100)]
		public string FirstName { get; set; }

		[StringLength(100)]
		public string LastName { get; set; }

		[StringLength(20)]
		[Phone]
		public string PhoneNumber { get; set; }

		[StringLength(200)]
		public string Address { get; set; }

		public bool IsAdmin { get; set; }

		// Navigation property
		public ICollection<TripRegistration> TripRegistrations { get; set; }
	}
}