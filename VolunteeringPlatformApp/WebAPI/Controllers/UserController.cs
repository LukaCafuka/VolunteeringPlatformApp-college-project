using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WebAPI.Dtos;
using WebAPI.Models;
using VolunteeringPlatformApp.Common.Security;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly VolunteerappContext _context;
        private readonly IConfiguration _configuration;

        public UserController(VolunteerappContext dbContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = dbContext;
        }

        [HttpPost("[action]")]
        public ActionResult<UserRegisterDto> Register(UserRegisterDto registerDto)
        {
            try
            {

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
    }
}
