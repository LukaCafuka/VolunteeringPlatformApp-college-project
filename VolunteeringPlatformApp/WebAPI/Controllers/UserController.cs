using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WebAPI.Dtos;
using WebAPI.Models;
using WebAPI.Services;
using VolunteeringPlatformApp.Common.Security;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly VolunteerappContext _context;
        private readonly IConfiguration _configuration;
        private readonly DbLoggingService _logger;

        public UserController(VolunteerappContext dbContext, IConfiguration configuration, DbLoggingService logger)
        {
            _configuration = configuration;
            _context = dbContext;
            _logger = logger;
        }

        [HttpPost("[action]")]
        public ActionResult<UserRegisterDto> Register(UserRegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var trimmedUsername = registerDto.Username.Trim();
                if (_context.AppUsers.Any(x => x.Username.Equals(trimmedUsername)))
                    return BadRequest($"Username {trimmedUsername} already exists");

                // Hash the password
                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(registerDto.Password, b64salt);

                var user = new AppUser
                {
                    Id = registerDto.Id,
                    Username = registerDto.Username,
                    PswdHash = b64hash,
                    PswdSalt = b64salt,
                    IsAdmin = registerDto.IsAdmin,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Email = registerDto.Email,
                };

                _context.Add(user);
                _context.SaveChanges();

                registerDto.Id = user.Id;

                return Ok(registerDto);

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("[action]")]
        public ActionResult Login(UserLoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var genericLoginFail = "Incorrect username or password";

                var existingUser = _context.AppUsers.FirstOrDefault(x => x.Username == loginDto.Username);
                if (existingUser == null)
                    return Unauthorized(genericLoginFail);

                var b64hash = PasswordHashProvider.GetHash(loginDto.Password, existingUser.PswdSalt);
                if (b64hash != existingUser.PswdHash)
                    return Unauthorized(genericLoginFail);

                var secureKey = _configuration["JWT:SecureKey"];
                var serializedToken = JwtTokenProvider.CreateToken(secureKey, 60, loginDto.Username, existingUser.IsAdmin ? "Admin" : "User", existingUser.Id.ToString());

                return Ok(serializedToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AppUserDto>>> GetAll()
        {
            try
            {
                var users = await _context.AppUsers
                    .Include(u => u.Projects)
                    .Select(u => new AppUserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FirstName = u.FirstName ?? string.Empty,
                        LastName = u.LastName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        IsAdmin = u.IsAdmin,
                        ProjectTitles = u.Projects.Select(p => p.Title).ToList()
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error getting users: {ex.Message}");
                return StatusCode(500, "Error retrieving users");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AppUserDto>> GetById(int id)
        {
            try
            {
                var user = await _context.AppUsers
                    .Include(u => u.Projects)
                    .Select(u => new AppUserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FirstName = u.FirstName ?? string.Empty,
                        LastName = u.LastName ?? string.Empty,
                        Email = u.Email ?? string.Empty,
                        IsAdmin = u.IsAdmin,
                        ProjectTitles = u.Projects.Select(p => p.Title).ToList()
                    })
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error getting user {id}: {ex.Message}");
                return StatusCode(500, "Error retrieving user");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<AppUserDto>> Update(int id, [FromBody] AppUserUpdateDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _context.AppUsers.FindAsync(id);
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                var existingUser = await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == updateDto.Username.ToLower().Trim() && u.Id != id);
                
                if (existingUser != null)
                {
                    return BadRequest($"Username '{updateDto.Username}' already exists");
                }

                user.Username = updateDto.Username.Trim();
                user.FirstName = updateDto.FirstName?.Trim();
                user.LastName = updateDto.LastName?.Trim();
                user.Email = updateDto.Email?.Trim();
                user.IsAdmin = updateDto.IsAdmin;

                if (!string.IsNullOrEmpty(updateDto.NewPassword))
                {
                    var b64salt = PasswordHashProvider.GetSalt();
                    var b64hash = PasswordHashProvider.GetHash(updateDto.NewPassword, b64salt);
                    user.PswdHash = b64hash;
                    user.PswdSalt = b64salt;
                }

                await _context.SaveChangesAsync();

                var updatedDto = new AppUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    IsAdmin = user.IsAdmin,
                    ProjectTitles = _context.Projects
                        .Where(p => p.Appusers.Any(u => u.Id == id))
                        .Select(p => p.Title)
                        .ToList()
                };

                await _logger.LogInformation($"User with ID {id} updated");
                
                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error updating user {id}: {ex.Message}");
                return StatusCode(500, "Error updating user");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var user = await _context.AppUsers
                    .Include(u => u.Projects)
                    .FirstOrDefaultAsync(u => u.Id == id);
                
                if (user == null)
                {
                    return NotFound($"User with ID {id} not found");
                }

                if (user.Projects.Any())
                {
                    return BadRequest($"Cannot delete user '{user.Username}' because they are participating in {user.Projects.Count} project(s). Remove from projects first.");
                }

                _context.AppUsers.Remove(user);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"User '{user.Username}' with ID {id} deleted");
                
                return NoContent();
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error deleting user {id}: {ex.Message}");
                return StatusCode(500, "Error deleting user");
            }
        }

        [HttpPost("{userId}/projects/{projectId}")]
        [Authorize]
        public async Task<ActionResult> AddUserToProject(int userId, int projectId)
        {
            try
            {
                var user = await _context.AppUsers
                    .Include(u => u.Projects)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found");
                }

                var project = await _context.Projects.FindAsync(projectId);
                if (project == null)
                {
                    return NotFound($"Project with ID {projectId} not found");
                }

                if (user.Projects.Any(p => p.Id == projectId))
                {
                    return BadRequest($"User '{user.Username}' is already assigned to project '{project.Title}'");
                }

                user.Projects.Add(project);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"User '{user.Username}' added to project '{project.Title}'");
                
                return Ok($"User '{user.Username}' successfully added to project '{project.Title}'");
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error adding user {userId} to project {projectId}: {ex.Message}");
                return StatusCode(500, "Error adding user to project");
            }
        }

        [HttpDelete("{userId}/projects/{projectId}")]
        [Authorize]
        public async Task<ActionResult> RemoveUserFromProject(int userId, int projectId)
        {
            try
            {
                var user = await _context.AppUsers
                    .Include(u => u.Projects)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found");
                }

                var project = user.Projects.FirstOrDefault(p => p.Id == projectId);
                if (project == null)
                {
                    return NotFound($"User '{user.Username}' is not assigned to project with ID {projectId}");
                }

                user.Projects.Remove(project);
                await _context.SaveChangesAsync();

                await _logger.LogInformation($"User '{user.Username}' removed from project '{project.Title}'");
                
                return Ok($"User '{user.Username}' successfully removed from project '{project.Title}'");
            }
            catch (Exception ex)
            {
                await _logger.LogError($"Error removing user {userId} from project {projectId}: {ex.Message}");
                return StatusCode(500, "Error removing user from project");
            }
        }
    }
}
