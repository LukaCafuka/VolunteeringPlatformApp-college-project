using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly VolunteerappContext _context;

        public LogController(VolunteerappContext context)
        {
            _context = context;
        }

        [HttpGet("get/{count}")]
        public async Task<ActionResult<IEnumerable<LogEntry>>> GetRecentLogs(int count)
        {
            return await _context.LogEntries
                .OrderByDescending(l => l.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        [HttpGet("count")]
        public async Task<ActionResult<int>> GetLogCount()
        {
            return await _context.LogEntries.CountAsync();
        }

    }
}
