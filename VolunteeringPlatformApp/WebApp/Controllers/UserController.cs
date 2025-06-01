using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.ViewModels;
using WebApp.Models;
using WebApp.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace WebApp.Controllers;

public class UserController : Controller
{
    private readonly VolunteerappContext _context;

    public UserController(VolunteerappContext context)
    {
        _context = context;
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

        return View();
    }

    [HttpPost]
    public IActionResult Login(LoginVM loginVm)
    {
        // Try to get a user from database
        var existingUser =
            _context
                .AppUsers
                .FirstOrDefault(x => x.Username == loginVm.Username);

        if (existingUser == null)
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View();
        }

        // Check if password hash matches
        var hash = PasswordHashProvider.GetHash(loginVm.Password, existingUser.PswdSalt);
        if (hash != existingUser.PswdHash)
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View();
        }

        var claims = new List<Claim>() {
            new Claim(ClaimTypes.Name, loginVm.Username),
            new Claim(ClaimTypes.Role, existingUser.IsAdmin ? "Admin" : "User"),
            new Claim("UserId", existingUser.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties();

        // We need to wrap async code here into synchronous since we don't use async methods
        Task.Run(async () =>
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties)
        ).GetAwaiter().GetResult();

        if (loginVm.ReturnUrl != null)
            return LocalRedirect(loginVm.ReturnUrl);
        else if (existingUser.IsAdmin)
            return RedirectToAction("Index", "Projects");
        else
            return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        Task.Run(async () =>
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme)
        ).GetAwaiter().GetResult();

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(UserVM userVm)
    {
        try
        {
            // Check if there is such a username in the database already
            var trimmedUsername = userVm.Username.Trim();
            if (_context.AppUsers.Any(x => x.Username.Equals(trimmedUsername)))
            {
                ModelState.AddModelError("Username", "Username is already taken");
                return View(userVm);
            }

            // Generate salt and hash password
            var salt = PasswordHashProvider.GetSalt();
            var hash = PasswordHashProvider.GetHash(userVm.Password, salt);

            // Create user from DTO and hashed password
            var user = new AppUser
            {
                Id = userVm.Id,
                Username = userVm.Username,
                PswdHash = hash,
                PswdSalt = salt,
                FirstName = userVm.FirstName,
                LastName = userVm.LastName,
                Email = userVm.Email,
                IsAdmin = false
            };

            // Add user and save changes to database
            _context.Add(user);
            _context.SaveChanges();

            // Update DTO Id to return it to the client
            userVm.Id = user.Id;

            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while registering. Please try again.");
            return View(userVm);
        }
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] WebApp.ViewModels.UserProfileVM vm)
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

        // Username uniqueness check (exclude self)
        var trimmedUsername = vm.Username.Trim();
        if (_context.AppUsers.Any(u => u.Id != vm.Id && u.Username.ToLower() == trimmedUsername.ToLower()))
        {
            ModelState.AddModelError("Username", "A user with this username already exists.");
            var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );
            return Json(new { success = false, errors });
        }

        user.Username = vm.Username;
        user.FirstName = vm.FirstName;
        user.LastName = vm.LastName;
        user.Email = vm.Email;
        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Profile updated successfully." });
    }
}

