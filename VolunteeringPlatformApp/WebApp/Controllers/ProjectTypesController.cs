using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Authorize]
    public class ProjectTypesController : BaseController
    {
        public ProjectTypesController(VolunteerappContext context) : base(context)
        {
        }

        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            return View(await _context.ProjectTypes.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            if (id == null)
            {
                return NotFound();
            }

            var projectType = await _context.ProjectTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectType == null)
            {
                return NotFound();
            }

            return View(projectType);
        }

        public async Task<IActionResult> Create()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] ProjectType projectType)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            var trimmedName = projectType.Name.Trim();
            if (_context.ProjectTypes.Any(pt => pt.Name.ToLower() == trimmedName.ToLower()))
            {
                ModelState.AddModelError("Name", "A project type with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(projectType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(projectType);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            if (id == null)
            {
                return NotFound();
            }

            var projectType = await _context.ProjectTypes.FindAsync(id);
            if (projectType == null)
            {
                return NotFound();
            }
            return View(projectType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] ProjectType projectType)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            if (id != projectType.Id)
            {
                return NotFound();
            }

            var trimmedName = projectType.Name.Trim();
            if (_context.ProjectTypes.Any(pt => pt.Id != id && pt.Name.ToLower() == trimmedName.ToLower()))
            {
                ModelState.AddModelError("Name", "A project type with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(projectType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectTypeExists(projectType.Id))
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
            return View(projectType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            if (id == null)
            {
                return NotFound();
            }

            var projectType = await _context.ProjectTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (projectType == null)
            {
                return NotFound();
            }

            return View(projectType);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            var projectType = await _context.ProjectTypes.FindAsync(id);
            if (projectType != null)
            {
                _context.ProjectTypes.Remove(projectType);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectTypeExists(int id)
        {
            return _context.ProjectTypes.Any(e => e.Id == id);
        }
    }
} 