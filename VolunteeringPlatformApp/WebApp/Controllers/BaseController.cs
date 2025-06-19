using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            // First try to get from JWT claims
            if (User.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    return await _context.AppUsers
                        .FirstOrDefaultAsync(u => u.Username == username);
                }
            }

            // Fallback to session if JWT is not available (during transition period)
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
            // First try to get from JWT claims
            if (User.Identity?.IsAuthenticated == true)
            {
                var isAdminClaim = User.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                if (!string.IsNullOrEmpty(isAdminClaim))
                {
                    return isAdminClaim.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                }
            }

            // Fallback to session or database lookup
            var user = await GetCurrentUser();
            return user?.IsAdmin == true;
        }

        protected int GetCurrentUserId()
        {
            // First try to get from JWT claims
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == "http://schemas.xmlsoap.org/2001/XMLSchema#string")?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int jwtUserId))
                {
                    return jwtUserId;
                }
            }

            // Fallback to session
            var sessionUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(sessionUserId) && int.TryParse(sessionUserId, out int sessionUserIdInt))
            {
                return sessionUserIdInt;
            }

            return 0;
        }
    }
} 