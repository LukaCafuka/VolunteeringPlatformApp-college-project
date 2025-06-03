using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly VolunteerappContext _context;

        public ProjectsController(VolunteerappContext context)
        {
            _context = context;
        }

        // GET: Projects
        public async Task<IActionResult> Index(string searchString, int? projectTypeId, int page = 1)
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

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Projects/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
            ViewBag.Skills = _context.Skills.ToList();
            return View();
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Project project, int[] selectedSkills)
        {
            // Only check if title is required
            var trimmedTitle = project.Title?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedTitle))
            {
                ModelState.AddModelError("Title", "Title is required.");
            }

            if (ModelState.IsValid)
            {
                project.Title = trimmedTitle; // Ensure we save the trimmed title
                if (selectedSkills != null)
                {
                    foreach (var skillId in selectedSkills)
                    {
                        var skill = await _context.Skills.FindAsync(skillId);
                        if (skill != null)
                        {
                            project.Skills.Add(skill);
                        }
                    }
                }

                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
            ViewBag.Skills = _context.Skills.ToList();
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Skills)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
            ViewBag.Skills = _context.Skills.ToList();
            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Project project, int[] selectedSkills)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            // Only check if title is required
            var trimmedTitle = project.Title?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedTitle))
            {
                ModelState.AddModelError("Title", "Title is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProject = await _context.Projects
                        .Include(p => p.Skills)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (existingProject == null)
                    {
                        return NotFound();
                    }

                    existingProject.Title = trimmedTitle; // Use the trimmed title
                    existingProject.Description = project.Description;
                    existingProject.ProjectTypeId = project.ProjectTypeId;

                    existingProject.Skills.Clear();
                    if (selectedSkills != null)
                    {
                        foreach (var skillId in selectedSkills)
                        {
                            var skill = await _context.Skills.FindAsync(skillId);
                            if (skill != null)
                            {
                                existingProject.Skills.Add(skill);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
            ViewBag.Skills = _context.Skills.ToList();
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Skills)
                .Include(p => p.Appusers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project != null)
            {
                // Remove project-skill relationships
                project.Skills.Clear();
                
                // Remove project-user relationships
                project.Appusers.Clear();
                
                // Save changes to remove relationships
                await _context.SaveChangesAsync();
                
                // Now remove the project
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
} 