using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.ViewModels;
using WebApp.Models;
using WebApp.Security;


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

        // Check is password hash matches
        var b64hash = PasswordHashProvider.GetHash(loginVm.Password, existingUser.PswdSalt);
        if (b64hash != existingUser.PswdHash)
        {
            ModelState.AddModelError("", "Invalid username or password");
            return View();
        }

        var claims = new List<Claim>() {
            new Claim(ClaimTypes.Name, loginVm.Username),
            new Claim(ClaimTypes.Role, existingUser.IsAdmin ? "Admin" : "User")
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
            return RedirectToAction("Index", "AdminHome");
        else
            return RedirectToAction("Index", "Home");
    }

    public IActionResult Logout()
    {
        Task.Run(async () =>
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme)
        ).GetAwaiter().GetResult();

        return View();
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
                return BadRequest($"Username {trimmedUsername} already exists");

            // Hash the password
            var b64salt = PasswordHashProvider.GetSalt();
            var b64hash = PasswordHashProvider.GetHash(userVm.Password, b64salt);

            // Create user from DTO and hashed password
            var user = new AppUser
            {
                Id = userVm.Id,
                Username = userVm.Username,
                PswdHash = b64hash,
                PswdSalt = b64salt,
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

            return RedirectToAction("Index", "Home");

        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

}

