using System;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
	public class Log
	{
		public int Id { get; set; }

		[Required]
		public DateTime Timestamp { get; set; } = DateTime.Now;

		[Required]
		[StringLength(50)]
		public string Level { get; set; }

		[Required]
		public string Message { get; set; }
	}
}