using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Dtos;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly VolunteerappContext _context;
        private readonly DbLoggingService _logger;

        public SkillController(VolunteerappContext context, DbLoggingService logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SkillDto>>> GetAll()
        {
            try
            {
                var skills = await _context.Skills
                    .Include(s => s.Projects)
                    .Select(s => new SkillDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        ProjectTitles = s.Projects.Select(p => p.Title).ToList()
                    })
                    .ToListAsync();

                return Ok(skills);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error getting skills: {ex.Message}");
                return StatusCode(500, "Error retrieving skills");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SkillDto>> GetById(int id)
        {
            try
            {
                var skill = await _context.Skills
                    .Include(s => s.Projects)
                    .Select(s => new SkillDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        ProjectTitles = s.Projects.Select(p => p.Title).ToList()
                    })
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (skill == null)
                {
                    return NotFound($"Skill with ID {id} not found");
                }

                return Ok(skill);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error getting skill {id}: {ex.Message}");
                return StatusCode(500, "Error retrieving skill");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<SkillDto>> Create([FromBody] SkillInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if name already exists
                var existingSkill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == inputDto.Name.ToLower().Trim());
                
                if (existingSkill != null)
                {
                    return BadRequest($"Skill with name '{inputDto.Name}' already exists");
                }

                var skill = new Skill
                {
                    Name = inputDto.Name.Trim(),
                    Description = inputDto.Description?.Trim()
                };

                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();

                var createdDto = new SkillDto
                {
                    Id = skill.Id,
                    Name = skill.Name,
                    Description = skill.Description,
                    ProjectTitles = new List<string>()
                };

                await _logger.LogInformation($"Skill '{skill.Name}' created with ID {skill.Id}");
                
                return CreatedAtAction(nameof(GetById), new { id = skill.Id }, createdDto);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error creating skill: {ex.Message}");
                return StatusCode(500, "Error creating skill");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<SkillDto>> Update(int id, [FromBody] SkillInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var skill = await _context.Skills.FindAsync(id);
                if (skill == null)
                {
                    return NotFound($"Skill with ID {id} not found");
                }

                var existingSkill = await _context.Skills
                    .FirstOrDefaultAsync(s => s.Name.ToLower() == inputDto.Name.ToLower().Trim() && s.Id != id);
                
                if (existingSkill != null)
                {
                    return BadRequest($"Skill with name '{inputDto.Name}' already exists");
                }

                skill.Name = inputDto.Name.Trim();
                skill.Description = inputDto.Description?.Trim();

                await _context.SaveChangesAsync();

                var updatedDto = new SkillDto
                {
                    Id = skill.Id,
                    Name = skill.Name,
                    Description = skill.Description,
                    ProjectTitles = _context.Projects
                        .Where(p => p.Skills.Any(s => s.Id == id))
                        .Select(p => p.Title)
                        .ToList()
                };

                await _logger.LogInformation($"Skill with ID {id} updated");
                
                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error updating skill {id}: {ex.Message}");
                return StatusCode(500, "Error updating skill");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var skill = await _context.Skills
                    .Include(s => s.Projects)
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (skill == null)
                {
                    return NotFound($"Skill with ID {id} not found");
                }

                if (skill.Projects.Any())
                {
                    return BadRequest($"Cannot delete Skill '{skill.Name}' because it is being used by {skill.Projects.Count} project(s)");
                }

                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"Skill '{skill.Name}' with ID {id} deleted");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error deleting skill {id}: {ex.Message}");
                return StatusCode(500, "Error deleting skill");
            }
        }
    }
} 