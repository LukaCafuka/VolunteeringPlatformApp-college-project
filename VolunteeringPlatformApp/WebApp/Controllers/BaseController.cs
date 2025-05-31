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
            if (User.Identity?.IsAuthenticated != true)
                return null;

            return await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
        }

        protected async Task<bool> IsAdmin()
        {
            var user = await GetCurrentUser();
            return user?.IsAdmin == true;
        }
    }
} 