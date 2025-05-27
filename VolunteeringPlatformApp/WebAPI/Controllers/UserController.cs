using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using WebAPI.Dtos;
using WebAPI.Models;
using WebAPI.Security;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly VolunteerappContext _dbContext;
        private readonly IConfiguration _configuration;

        public UserController(VolunteerappContext dbContext, IConfiguration configuration)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("[action]")]
        public ActionResult<UserRegisterDto> Register(UserRegisterDto registerDto)
        {
            try
            {
                // Check if there is such a username in the database already
                var trimmedUsername = registerDto.Username.Trim();
                if (_dbContext.AppUsers.Any(x => x.Username.Equals(trimmedUsername)))
                    return BadRequest($"Username {trimmedUsername} already exists");

                // Hash the password
                var b64salt = PasswordHashProvider.GetSalt();
                var b64hash = PasswordHashProvider.GetHash(registerDto.Password, b64salt);

                // Create user from DTO and hashed password
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

                // Add user and save changes to database
                _dbContext.Add(user);
                _dbContext.SaveChanges();

                // Update DTO Id to return it to the client
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

                // Try to get a user from database
                var existingUser = _dbContext.AppUsers.FirstOrDefault(x => x.Username == loginDto.Username);
                if (existingUser == null)
                    return Unauthorized(genericLoginFail);

                // Check is password hash matches
                var b64hash = PasswordHashProvider.GetHash(loginDto.Password, existingUser.PswdSalt);
                if (b64hash != existingUser.PswdHash)
                    return Unauthorized(genericLoginFail);

                var secureKey = _configuration["JWT:SecureKey"];
                var serializedToken = JwtTokenProvider.CreateToken(secureKey, 60, loginDto.Username, "User");

                return Ok(serializedToken);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
