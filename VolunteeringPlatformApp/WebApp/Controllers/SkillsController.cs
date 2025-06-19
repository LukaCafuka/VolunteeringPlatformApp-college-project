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

        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin())
            {
                return RedirectToAction("Index", "Projects");
            }

            return View(await _context.Skills.ToListAsync());
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

            var skill = await _context.Skills
                .FirstOrDefaultAsync(m => m.Id == id);
            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
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