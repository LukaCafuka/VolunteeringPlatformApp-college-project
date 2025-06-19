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
    //[Authorize]
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
        public async Task<ActionResult<ProjectDto>>Create([FromBody] ProjectInputDto inputDto)
        {
            try
            {
                if (inputDto == null)
                {
                    return BadRequest("Project data is invalid.");
                }

                var project = new Project
                {
                    Title = inputDto.Title,
                    Description = inputDto.Description,
                    ProjectTypeId = inputDto.ProjectTypeId,
                    CreatedAt = DateTime.UtcNow
                };

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

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"Project with id={project.Id} created");
                return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "There has been a problem while creating the project");
            }
        }

        [HttpPut("[action]/{id}")]
        public async Task<ActionResult<ProjectDto>> Update(int id, [FromBody] ProjectInputDto inputDto)
        {
            try
            {
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
                    .Select(p => new ProjectDto {
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
    }
}
