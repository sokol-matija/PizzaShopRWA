using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Services
{
	public interface ILogService
	{
		Task LogInformationAsync(string message);
		Task LogWarningAsync(string message);
		Task LogErrorAsync(string message);
		Task<List<LogDTO>> GetLogsAsync(int count);
		Task<int> GetLogsCountAsync();
	}

	public class LogService : ILogService
	{
		private readonly ApplicationDbContext _context;

		public LogService(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task LogInformationAsync(string message)
		{
			await AddLogAsync("Information", message);
		}

		public async Task LogWarningAsync(string message)
		{
			await AddLogAsync("Warning", message);
		}

		public async Task LogErrorAsync(string message)
		{
			await AddLogAsync("Error", message);
		}

		private async Task AddLogAsync(string level, string message)
		{
			try
			{
				var log = new Log
				{
					Timestamp = DateTime.Now,
					Level = level,
					Message = message
				};

				_context.Logs.Add(log);
				await _context.SaveChangesAsync();
			}
			catch (Exception)
			{
				// Silently fail because we don't want logging errors to disrupt the flow
				// In production, you might want to use a more robust logging system
			}
		}

		public async Task<List<LogDTO>> GetLogsAsync(int count)
		{
			return await _context.Logs
				.OrderByDescending(l => l.Timestamp)
				.Take(count)
				.Select(l => new LogDTO
				{
					Id = l.Id,
					Timestamp = l.Timestamp,
					Level = l.Level,
					Message = l.Message
				})
				.ToListAsync();
		}

		public async Task<int> GetLogsCountAsync()
		{
			return await _context.Logs.CountAsync();
		}
	}
}