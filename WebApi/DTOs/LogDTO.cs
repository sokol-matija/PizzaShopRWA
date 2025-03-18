using System;

namespace WebAPI.DTOs
{
	public class LogDTO
	{
		public int Id { get; set; }
		public DateTime Timestamp { get; set; }
		public string Level { get; set; }
		public string Message { get; set; }
	}
}