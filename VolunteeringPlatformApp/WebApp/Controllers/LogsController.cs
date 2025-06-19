using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using VolunteeringPlatformApp.Common.Models;

namespace WebApp.Controllers
{
    [Authorize]
    public class LogsController : BaseController
    {
        public LogsController(VolunteerappContext context) : base(context)
        {
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (!await IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            const int pageSize = 20;

            // Get total count for pagination
            var totalLogs = await _context.LogEntries.CountAsync();

            // Get logs for current page
            var logs = await _context.LogEntries
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalLogs / pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View(logs);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!await IsAdmin())
            {
                TempData["Error"] = "Access denied. Admin privileges required.";
                return RedirectToAction("Index", "Home");
            }

            var logEntry = await _context.LogEntries.FindAsync(id);
            if (logEntry == null)
            {
                TempData["Error"] = "Log entry not found.";
                return RedirectToAction("Index");
            }

            return View(logEntry);
        }
    }
} 