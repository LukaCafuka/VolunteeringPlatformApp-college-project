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

        // GET: ProjectTypes
        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            return View(await _context.ProjectTypes.ToListAsync());
        }

        // GET: ProjectTypes/Details/5
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

        // GET: ProjectTypes/Create
        public async Task<IActionResult> Create()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            return View();
        }

        // POST: ProjectTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] ProjectType projectType)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            if (ModelState.IsValid)
            {
                _context.Add(projectType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(projectType);
        }

        // GET: ProjectTypes/Edit/5
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

        // POST: ProjectTypes/Edit/5
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

        // GET: ProjectTypes/Delete/5
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

        // POST: ProjectTypes/Delete/5
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