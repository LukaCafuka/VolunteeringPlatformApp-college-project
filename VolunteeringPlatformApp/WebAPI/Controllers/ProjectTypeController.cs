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
    public class ProjectTypeController : ControllerBase
    {
        private readonly VolunteerappContext _context;
        private readonly DbLoggingService _logger;

        public ProjectTypeController(VolunteerappContext context, DbLoggingService logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectTypeDto>>> GetAll()
        {
            try
            {
                var projectTypes = await _context.ProjectTypes
                    .Include(pt => pt.Projects)
                    .Select(pt => new ProjectTypeDto
                    {
                        Id = pt.Id,
                        Name = pt.Name,
                        Description = pt.Description,
                        ProjectTitles = pt.Projects.Select(p => p.Title).ToList()
                    })
                    .ToListAsync();

                return Ok(projectTypes);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error getting project types: {ex.Message}");
                return StatusCode(500, "Error retrieving project types");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectTypeDto>> GetById(int id)
        {
            try
            {
                var projectType = await _context.ProjectTypes
                    .Include(pt => pt.Projects)
                    .Select(pt => new ProjectTypeDto
                    {
                        Id = pt.Id,
                        Name = pt.Name,
                        Description = pt.Description,
                        ProjectTitles = pt.Projects.Select(p => p.Title).ToList()
                    })
                    .FirstOrDefaultAsync(pt => pt.Id == id);

                if (projectType == null)
                {
                    return NotFound($"ProjectType with ID {id} not found");
                }

                return Ok(projectType);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error getting project type {id}: {ex.Message}");
                return StatusCode(500, "Error retrieving project type");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProjectTypeDto>> Create([FromBody] ProjectTypeInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if name already exists
                var existingProjectType = await _context.ProjectTypes
                    .FirstOrDefaultAsync(pt => pt.Name.ToLower() == inputDto.Name.ToLower().Trim());
                
                if (existingProjectType != null)
                {
                    return BadRequest($"ProjectType with name '{inputDto.Name}' already exists");
                }

                var projectType = new ProjectType
                {
                    Name = inputDto.Name.Trim(),
                    Description = inputDto.Description?.Trim()
                };

                _context.ProjectTypes.Add(projectType);
                await _context.SaveChangesAsync();

                var createdDto = new ProjectTypeDto
                {
                    Id = projectType.Id,
                    Name = projectType.Name,
                    Description = projectType.Description,
                    ProjectTitles = new List<string>()
                };

                await _logger.LogInformation($"ProjectType '{projectType.Name}' created with ID {projectType.Id}");
                
                return CreatedAtAction(nameof(GetById), new { id = projectType.Id }, createdDto);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error creating project type: {ex.Message}");
                return StatusCode(500, "Error creating project type");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ProjectTypeDto>> Update(int id, [FromBody] ProjectTypeInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var projectType = await _context.ProjectTypes.FindAsync(id);
                if (projectType == null)
                {
                    return NotFound($"ProjectType with ID {id} not found");
                }

                var existingProjectType = await _context.ProjectTypes
                    .FirstOrDefaultAsync(pt => pt.Name.ToLower() == inputDto.Name.ToLower().Trim() && pt.Id != id);
                
                if (existingProjectType != null)
                {
                    return BadRequest($"ProjectType with name '{inputDto.Name}' already exists");
                }

                projectType.Name = inputDto.Name.Trim();
                projectType.Description = inputDto.Description?.Trim();

                await _context.SaveChangesAsync();

                var updatedDto = new ProjectTypeDto
                {
                    Id = projectType.Id,
                    Name = projectType.Name,
                    Description = projectType.Description,
                    ProjectTitles = _context.Projects.Where(p => p.ProjectTypeId == id).Select(p => p.Title).ToList()
                };

                await _logger.LogInformation($"ProjectType with ID {id} updated");
                
                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error updating project type {id}: {ex.Message}");
                return StatusCode(500, "Error updating project type");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var projectType = await _context.ProjectTypes
                    .Include(pt => pt.Projects)
                    .FirstOrDefaultAsync(pt => pt.Id == id);
                
                if (projectType == null)
                {
                    return NotFound($"ProjectType with ID {id} not found");
                }

                if (projectType.Projects.Any())
                {
                    return BadRequest($"Cannot delete ProjectType '{projectType.Name}' because it is being used by {projectType.Projects.Count} project(s)");
                }

                _context.ProjectTypes.Remove(projectType);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"ProjectType '{projectType.Name}' with ID {id} deleted");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error deleting project type {id}: {ex.Message}");
                return StatusCode(500, "Error deleting project type");
            }
        }
    }
} 