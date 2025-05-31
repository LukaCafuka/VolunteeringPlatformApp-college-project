using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.Controllers
{
    [Authorize]
    public class SkillsController : BaseController
    {
        public SkillsController(VolunteerappContext context) : base(context)
        {
        }

        // GET: Skills
        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            return View(await _context.Skills.ToListAsync());
        }

        // GET: Skills/Details/5
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

            var skill = await _context.Skills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // GET: Skills/Create
        public async Task<IActionResult> Create()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            return View();
        }

        // POST: Skills/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Skill skill)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            // Unique name check
            var trimmedName = skill.Name.Trim();
            if (_context.Skills.Any(s => s.Name.ToLower() == trimmedName.ToLower()))
            {
                ModelState.AddModelError("Name", "A skill with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        // GET: Skills/Edit/5
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

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                return NotFound();
            }
            return View(skill);
        }

        // POST: Skills/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Skill skill)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            if (id != skill.Id)
            {
                return NotFound();
            }

            // Unique name check (exclude self)
            var trimmedName = skill.Name.Trim();
            if (_context.Skills.Any(s => s.Id != id && s.Name.ToLower() == trimmedName.ToLower()))
            {
                ModelState.AddModelError("Name", "A skill with this name already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillExists(skill.Id))
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
            return View(skill);
        }

        // GET: Skills/Delete/5
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

            var skill = await _context.Skills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // POST: Skills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            var skill = await _context.Skills.FindAsync(id);
            if (skill != null)
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool SkillExists(int id)
        {
            return _context.Skills.Any(e => e.Id == id);
        }
    }
} 