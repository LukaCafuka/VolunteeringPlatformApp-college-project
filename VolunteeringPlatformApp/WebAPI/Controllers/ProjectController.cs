using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.Dtos;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly VolunteerappContext _context;
        private readonly DbLoggingService _logger;

        public ProjectController(VolunteerappContext dbContext, DbLoggingService logger)
        {
            _context = dbContext;
            _logger = logger;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAll()
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.ProjectType)
                    .Include(p => p.Skills)
                    .Include(p => p.Appusers)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        ProjectTypeName = p.ProjectType != null ? p.ProjectType.Name : null,
                        SkillNames = p.Skills.Select(s => s.Name).ToList(),
                        AppUserNames = p.Appusers.Select(u => u.Username).ToList()
                    })
                    .ToListAsync();


                return Ok(projects);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "There has been a problem while fetching the data you requested");
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ProjectDto>> GetById(int id)
        {
            try
            {
                var project = await _context.Projects
                .Include(p => p.ProjectType)
                .Include(p => p.Skills)
                .Include(p => p.Appusers)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    ProjectTypeName = p.ProjectType != null ? p.ProjectType.Name : null,
                    SkillNames = p.Skills.Select(s => s.Name).ToList(),
                    AppUserNames = p.Appusers.Select(u => u.Username).ToList()
                })
                .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }

                return Ok(project);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "There has been a problem while fetching the data you requested");
            }
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<ActionResult<ProjectDto>> Create([FromBody] ProjectInputDto inputDto)
        {
            try
            {
                if (inputDto == null)
                {
                    return BadRequest("Project data is required.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (inputDto.ProjectTypeId.HasValue)
                {
                    var projectTypeExists = await _context.ProjectTypes
                        .AnyAsync(pt => pt.Id == inputDto.ProjectTypeId.Value);
                    
                    if (!projectTypeExists)
                    {
                        return BadRequest($"ProjectType with ID {inputDto.ProjectTypeId} does not exist.");
                    }
                }

                if (inputDto.SkillIds != null && inputDto.SkillIds.Any())
                {
                    var existingSkillIds = await _context.Skills
                        .Where(s => inputDto.SkillIds.Contains(s.Id))
                        .Select(s => s.Id)
                        .ToListAsync();
                    
                    var invalidSkillIds = inputDto.SkillIds.Except(existingSkillIds).ToList();
                    if (invalidSkillIds.Any())
                    {
                        return BadRequest($"Skills with IDs [{string.Join(", ", invalidSkillIds)}] do not exist.");
                    }
                }

                if (inputDto.AppUserIds != null && inputDto.AppUserIds.Any())
                {
                    var existingUserIds = await _context.AppUsers
                        .Where(u => inputDto.AppUserIds.Contains(u.Id))
                        .Select(u => u.Id)
                        .ToListAsync();
                    
                    var invalidUserIds = inputDto.AppUserIds.Except(existingUserIds).ToList();
                    if (invalidUserIds.Any())
                    {
                        return BadRequest($"Users with IDs [{string.Join(", ", invalidUserIds)}] do not exist.");
                    }
                }


                var project = new Project
                {
                    Title = inputDto.Title.Trim(),
                    Description = inputDto.Description?.Trim(),
                    ProjectTypeId = inputDto.ProjectTypeId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Projects.Add(project);

                if (inputDto.SkillIds != null && inputDto.SkillIds.Any())
                {
                    project.Skills = await _context.Skills
                        .Where(s => inputDto.SkillIds.Contains(s.Id))
                        .ToListAsync();
                }

                if (inputDto.AppUserIds != null && inputDto.AppUserIds.Any())
                {
                    project.Appusers = await _context.AppUsers
                        .Where(u => inputDto.AppUserIds.Contains(u.Id))
                        .ToListAsync();
                }

                await _context.SaveChangesAsync();

                var createdProject = await _context.Projects
                    .Include(p => p.ProjectType)
                    .Include(p => p.Skills)
                    .Include(p => p.Appusers)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        ProjectTypeName = p.ProjectType != null ? p.ProjectType.Name : null,
                        SkillNames = p.Skills.Select(s => s.Name).ToList(),
                        AppUserNames = p.Appusers.Select(u => u.Username).ToList()
                    })
                    .FirstOrDefaultAsync(p => p.Id == project.Id);

                await _logger.LogInformation($"Project '{project.Title}' with id={project.Id} created successfully");
                
                return CreatedAtAction(nameof(GetById), new { id = project.Id }, createdProject);
            }
            catch (DbUpdateException dbEx)
            {
                await _logger.LogError($"Database error while creating project: {dbEx.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "There was a database error while creating the project. Please check if all referenced entities exist.");
            }
            catch (InvalidOperationException invEx)
            {
                await _logger.LogError($"Invalid operation while creating project: {invEx.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "There was an invalid operation while creating the project.");
            }
            catch (ArgumentException argEx)
            {
                await _logger.LogError($"Argument error while creating project: {argEx.Message}");
                return BadRequest($"Invalid input: {argEx.Message}");
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Unexpected error while creating project: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "There has been an unexpected problem while creating the project");
            }
        }

                [HttpPut("[action]/{id}")]
        [Authorize]
        public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] ProjectInputDto inputDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingProject = await _context.Projects
            .Include(p => p.Skills)
            .Include(p => p.Appusers)
            .FirstOrDefaultAsync(p => p.Id == id);

                if (existingProject == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }

                existingProject.Title = inputDto.Title;
                existingProject.Description = inputDto.Description;
                existingProject.ProjectTypeId = inputDto.ProjectTypeId;

                if (inputDto.SkillIds != null)
                {
                    existingProject.Skills = await _context.Skills
                        .Where(s => inputDto.SkillIds.Contains(s.Id))
                        .ToListAsync();
                }

                if (inputDto.AppUserIds != null)
                {
                    existingProject.Appusers = await _context.AppUsers
                        .Where(u => inputDto.AppUserIds.Contains(u.Id))
                        .ToListAsync();
                }

                _context.Entry(existingProject).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"Project with id={existingProject.Id} updated");
                return Ok(existingProject);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "There has been a problem while updating the project");
            }
        }

        [HttpDelete("[action]/{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var project = await _context.Projects
                .Include(p => p.Skills)
                .Include(p => p.Appusers)
                .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    return NotFound($"Project with ID {id} not found.");
                }

                project.Skills.Clear();
                project.Appusers.Clear();

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"Project with id={project.Id} deleted");
                return Ok($"Project with ID {id} has been deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    $"There has been a problem while deleting the project: '{ex.Message}'");
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> Search(
        string? searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var query = _context.Projects
                    .Include(p => p.ProjectType)
                    .Include(p => p.Skills)
                    .Include(p => p.Appusers)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(p => p.Title.Contains(searchTerm)
                        || p.Description.Contains(searchTerm));
                }

                var totalItems = await query.CountAsync();
                var projects = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProjectDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        CreatedAt = p.CreatedAt,
                        ProjectTypeName = p.ProjectType != null ? p.ProjectType.Name : null,
                        SkillNames = p.Skills.Select(s => s.Name).ToList(),
                        AppUserNames = p.Appusers.Select(u => u.Username).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Items = projects
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error during search");
            }
        }

        [HttpPost("{projectId}/skills/{skillId}")]
        [Authorize]
        public async Task<ActionResult> AddSkillToProject(int projectId, int skillId)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Skills)
                    .FirstOrDefaultAsync(p => p.Id == projectId);
                
                if (project == null)
                {
                    return NotFound($"Project with ID {projectId} not found");
                }

                var skill = await _context.Skills.FindAsync(skillId);
                if (skill == null)
                {
                    return NotFound($"Skill with ID {skillId} not found");
                }

                if (project.Skills.Any(s => s.Id == skillId))
                {
                    return BadRequest($"Skill '{skill.Name}' is already associated with project '{project.Title}'");
                }

                project.Skills.Add(skill);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"Skill '{skill.Name}' added to project '{project.Title}'");
                
                return Ok($"Skill '{skill.Name}' successfully added to project '{project.Title}'");
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error adding skill {skillId} to project {projectId}: {ex.Message}");
                return StatusCode(500, "Error adding skill to project");
            }
        }

        [HttpDelete("{projectId}/skills/{skillId}")]
        [Authorize]
        public async Task<ActionResult> RemoveSkillFromProject(int projectId, int skillId)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Skills)
                    .FirstOrDefaultAsync(p => p.Id == projectId);
                
                if (project == null)
                {
                    return NotFound($"Project with ID {projectId} not found");
                }

                var skill = project.Skills.FirstOrDefault(s => s.Id == skillId);
                if (skill == null)
                {
                    return NotFound($"Skill with ID {skillId} is not associated with project '{project.Title}'");
                }

                project.Skills.Remove(skill);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"Skill '{skill.Name}' removed from project '{project.Title}'");
                
                return Ok($"Skill '{skill.Name}' successfully removed from project '{project.Title}'");
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error removing skill {skillId} from project {projectId}: {ex.Message}");
                return StatusCode(500, "Error removing skill from project");
            }
        }
    }
}
