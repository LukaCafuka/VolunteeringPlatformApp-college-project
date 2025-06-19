using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.ViewModels;
using WebApp.Models;
using VolunteeringPlatformApp.Common.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers;

public class UserController : BaseController
{
    public UserController(VolunteerappContext context) : base(context)
    {
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Login(string returnUrl)
    {
        var loginVm = new LoginVM
        {
            ReturnUrl = returnUrl
        };

        return View(loginVm);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVM loginVm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(loginVm);
            }

            var existingUser = await _context.AppUsers
                .FirstOrDefaultAsync(x => x.Username == loginVm.Username);

            if (existingUser == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(loginVm);
            }

            var hash = PasswordHashProvider.GetHash(loginVm.Password, existingUser.PswdSalt);
            if (hash != existingUser.PswdHash)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(loginVm);
            }

            var secureKey = HttpContext.RequestServices.GetService<IConfiguration>()["JWT:SecureKey"];
            var token = JwtTokenProvider.CreateToken(secureKey, 60, loginVm.Username, existingUser.IsAdmin ? "Admin" : "User", existingUser.Id.ToString());

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(60)
            });

            HttpContext.Session.SetString("Username", existingUser.Username);
            HttpContext.Session.SetString("IsAdmin", existingUser.IsAdmin.ToString());
            HttpContext.Session.SetString("UserId", existingUser.Id.ToString());

            if (!string.IsNullOrEmpty(loginVm.ReturnUrl))
                return LocalRedirect(loginVm.ReturnUrl);
            else if (existingUser.IsAdmin)
                return RedirectToAction("Index", "Projects");
            else
                return RedirectToAction("Index", "Home");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", "Database error occurred. Please try again later.");
            Console.WriteLine($"Database error in Login: {ex.Message}");
            return View(loginVm);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("", "Configuration error. Please contact support.");
            Console.WriteLine($"Configuration error in Login: {ex.Message}");
            return View(loginVm);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", "Invalid input provided.");
            Console.WriteLine($"Argument error in Login: {ex.Message}");
            return View(loginVm);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
            Console.WriteLine($"Unexpected error in Login: {ex.Message}");
            return View(loginVm);
        }
    }

    public IActionResult Logout()
    {
        // Remove JWT token cookie
        Response.Cookies.Delete("access_token");
        
        // Clear session
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserVM userVm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(userVm);
            }

            var trimmedUsername = userVm.Username.Trim();
            var existingUser = await _context.AppUsers
                .AnyAsync(x => x.Username.ToLower() == trimmedUsername.ToLower());

            if (existingUser)
            {
                ModelState.AddModelError("Username", "Username is already taken");
                return View(userVm);
            }

            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash(userVm.Password, salt);

            var user = new AppUser
            {
                Id = userVm.Id,
                Username = trimmedUsername,
                PswdHash = hash,
                PswdSalt = salt,
                FirstName = userVm.FirstName,
                LastName = userVm.LastName,
                Email = userVm.Email,
                IsAdmin = false
            };

            _context.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registration successful! Please log in.";
            return RedirectToAction("Login");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError("", "Database error occurred during registration. Please try again.");
            Console.WriteLine($"Database error in Register: {ex.Message}");
            return View(userVm);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", "Invalid data provided. Please check your input.");
            Console.WriteLine($"Argument error in Register: {ex.Message}");
            return View(userVm);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An unexpected error occurred during registration. Please try again.");
            Console.WriteLine($"Unexpected error in Register: {ex.Message}");
            return View(userVm);
        }
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        try
        {
            var user = await GetCurrentUser();
            if (user == null)
                return NotFound();

            var vm = new WebApp.ViewModels.UserProfileVM
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsAdmin = user.IsAdmin
            };
            return View(vm);
        }
        catch (DbUpdateException ex)
        {
            TempData["Error"] = "Database error occurred. Please try again later.";
            Console.WriteLine($"Database error in Profile: {ex.Message}");
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "An unexpected error occurred. Please try again later.";
            Console.WriteLine($"Unexpected error in Profile: {ex.Message}");
            return RedirectToAction("Index", "Home");
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] WebApp.ViewModels.UserProfileVM vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return Json(new { success = false, errors });
            }

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == vm.Id);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            var trimmedUsername = vm.Username.Trim();
            var usernameExists = await _context.AppUsers
                .AnyAsync(u => u.Id != vm.Id && u.Username.ToLower() == trimmedUsername.ToLower());

            if (usernameExists)
            {
                ModelState.AddModelError("Username", "A user with this username already exists.");
                var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return Json(new { success = false, errors });
            }

            user.Username = trimmedUsername;
            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Email = vm.Email;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Profile updated successfully." });
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Database error in UpdateProfile: {ex.Message}");
            return Json(new { success = false, message = "Database error occurred. Please try again later." });
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Argument error in UpdateProfile: {ex.Message}");
            return Json(new { success = false, message = "Invalid data provided." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error in UpdateProfile: {ex.Message}");
            return Json(new { success = false, message = "An unexpected error occurred. Please try again later." });
        }
    }
}

