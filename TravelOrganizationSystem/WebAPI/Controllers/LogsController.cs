using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebAPI.Services;

namespace WebAPI.Controllers
{
	/// <summary>
	/// Controller for managing and retrieving system logs
	/// </summary>
	/// <remarks>
	/// All endpoints in this controller require Admin role access
	/// </remarks>
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin")]
	public class LogsController : ControllerBase
	{
		private readonly ILogService _logService;

		public LogsController(ILogService logService)
		{
			_logService = logService;
		}

		/// <summary>
		/// Get the most recent logs up to the specified count
		/// </summary>
		/// <param name="count">Number of log entries to retrieve</param>
		/// <remarks>
		/// This endpoint requires Admin role access
		/// </remarks>
		/// <returns>The requested number of most recent log entries</returns>
		[HttpGet("get/{count}")]
		public async Task<IActionResult> Get(int count)
		{
			if (count <= 0)
				return BadRequest("Count must be greater than 0");

			var logs = await _logService.GetLogsAsync(count);
			return Ok(logs);
		}

		/// <summary>
		/// Get the total count of log entries in the system
		/// </summary>
		/// <remarks>
		/// This endpoint requires Admin role access
		/// </remarks>
		/// <returns>The total count of log entries</returns>
		[HttpGet("count")]
		public async Task<IActionResult> Count()
		{
			var count = await _logService.GetLogsCountAsync();
			return Ok(new { count });
		}
	}
}