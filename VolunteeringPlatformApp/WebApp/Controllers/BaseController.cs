using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApp.Models;

namespace WebApp.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly VolunteerappContext _context;

        protected BaseController(VolunteerappContext context)
        {
            _context = context;
        }

        protected async Task<AppUser?> GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    return await _context.AppUsers
                        .FirstOrDefaultAsync(u => u.Username == username);
                }
            }

            var sessionUsername = HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(sessionUsername))
            {
                return await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username == sessionUsername);
            }

            return null;
        }

        protected async Task<bool> IsAdmin()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                if (!string.IsNullOrEmpty(isAdminClaim))
                {
                    return isAdminClaim.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                }
            }

            var user = await GetCurrentUser();
            return user?.IsAdmin == true;
        }

        protected int GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId" || c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int jwtUserId))
                {
                    return jwtUserId;
                }
            }

            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int sessionUserIdInt))
            {
                return sessionUserIdInt;
            }

            return 0;
        }
    }
} 