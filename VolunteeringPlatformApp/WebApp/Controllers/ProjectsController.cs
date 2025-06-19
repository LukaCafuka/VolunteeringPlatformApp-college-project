using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize]
    public class ProjectsController : BaseController
    {
        public ProjectsController(VolunteerappContext context) : base(context)
        {
        }

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

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
            ViewBag.Skills = _context.Skills.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Project project, int[] selectedSkills)
        {
            try
            {
                var trimmedTitle = project.Title?.Trim();
                if (string.IsNullOrWhiteSpace(trimmedTitle))
                {
                    ModelState.AddModelError("Title", "Title is required.");
                }

                if (ModelState.IsValid)
                {
                    project.Title = trimmedTitle;
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
                    TempData["Success"] = "Project created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
                ViewBag.Skills = _context.Skills.ToList();
                return View(project);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Database error occurred while creating project. Please try again.");
                Console.WriteLine($"Database error in Create: {ex.Message}");
                ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
                ViewBag.Skills = _context.Skills.ToList();
                return View(project);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", "Invalid data provided. Please check your input.");
                Console.WriteLine($"Argument error in Create: {ex.Message}");
                ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
                ViewBag.Skills = _context.Skills.ToList();
                return View(project);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                Console.WriteLine($"Unexpected error in Create: {ex.Message}");
                ViewBag.ProjectTypes = _context.ProjectTypes.ToList();
                ViewBag.Skills = _context.Skills.ToList();
                return View(project);
            }
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Project project, int[] selectedSkills)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

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

                    existingProject.Title = trimmedTitle;
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Skills)
                    .Include(p => p.Appusers)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project != null)
                {
                    project.Skills.Clear();
                    project.Appusers.Clear();

                    await _context.SaveChangesAsync();

                    _context.Projects.Remove(project);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Project deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Project not found.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Database error occurred while deleting project. Please try again.";
                Console.WriteLine($"Database error in DeleteConfirmed: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = "Cannot delete project due to existing dependencies.";
                Console.WriteLine($"Operation error in DeleteConfirmed: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred while deleting project.";
                Console.WriteLine($"Unexpected error in DeleteConfirmed: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
} 