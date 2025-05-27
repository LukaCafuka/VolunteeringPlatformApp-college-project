using WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Services
{
    public class DbLoggingService : ILoggingService
    {
        private readonly VolunteerappContext _context;

        public DbLoggingService(VolunteerappContext context)
        {
            _context = context;
        }

        public async Task LogInformation(string message)
        {
            await LogAsync("Information", message);
        }

        public async Task LogError(string message)
        {
            await LogAsync("Error", message);
        }

        private async Task LogAsync(string level, string message)
        {
            var log = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message
            };

            _context.LogEntries.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<LogEntry>> GetRecentLogs(int count)
        {
            return await _context.LogEntries
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetTotalLogCount()
        {
            return await _context.LogEntries.CountAsync();
        }
    }
}
