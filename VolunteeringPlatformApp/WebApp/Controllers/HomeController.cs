using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly VolunteerappContext _context;

    public HomeController(ILogger<HomeController> logger, VolunteerappContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Projects");
            }
            else
            {
                return RedirectToAction("AvailableProjects");
            }
        }
        return View();
    }

    [Authorize]
    public async Task<IActionResult> AvailableProjects(string searchString, int? projectTypeId, int page = 1)
    {
        var projects = _context.Projects
            .Include(p => p.ProjectType)
            .Include(p => p.Skills)
            .Include(p => p.Appusers)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            projects = projects.Where(p => p.Title.Contains(searchString) || 
                                         p.Description.Contains(searchString));
        }

        if (projectTypeId.HasValue)
        {
            projects = projects.Where(p => p.ProjectTypeId == projectTypeId);
        }

        var pageSize = 10;
        var totalProjects = await projects.CountAsync();
        var totalPages = (int)Math.Ceiling(totalProjects / (double)pageSize);

        var projectsList = await projects
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.ProjectTypes = await _context.ProjectTypes.ToListAsync();
        ViewBag.CurrentSearchString = searchString;
        ViewBag.CurrentProjectTypeId = projectTypeId;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

        return View(projectsList);
    }

    [Authorize]
    public async Task<IActionResult> ProjectDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var project = await _context.Projects
            .Include(p => p.ProjectType)
            .Include(p => p.Skills)
            .Include(p => p.Appusers)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> JoinProject(int projectId)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var project = await _context.Projects
            .Include(p => p.Appusers)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return NotFound();
        }

        var user = await _context.AppUsers.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        if (!project.Appusers.Contains(user))
        {
            project.Appusers.Add(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = "You have successfully joined the project!";
        }
        else
        {
            TempData["Error"] = "You are already a volunteer for this project.";
        }

        return RedirectToAction(nameof(ProjectDetails), new { id = projectId });
    }

    [Authorize]
    public async Task<IActionResult> MyProjects()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var user = await _context.AppUsers
            .Include(u => u.Projects)
            .ThenInclude(p => p.ProjectType)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound();
        }

        return View(user.Projects);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UserActivities()
    {
        var users = await _context.AppUsers
            .Include(u => u.Projects)
            .ThenInclude(p => p.ProjectType)
            .ToListAsync();

        return View(users);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
