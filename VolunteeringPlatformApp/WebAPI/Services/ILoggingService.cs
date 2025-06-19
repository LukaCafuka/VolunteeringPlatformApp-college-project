using WebAPI.Models;
using VolunteeringPlatformApp.Common.Models;

namespace WebAPI.Services
{
    public interface ILoggingService
    {
        Task LogInformation(string message);
        Task LogError(string message);
        Task<IEnumerable<LogEntry>> GetRecentLogs(int count);
        Task<int> GetTotalLogCount();
    }
}
