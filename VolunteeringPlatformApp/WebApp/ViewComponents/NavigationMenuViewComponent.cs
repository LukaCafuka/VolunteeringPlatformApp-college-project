using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;

namespace WebApp.ViewComponents
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        private readonly VolunteerappContext _context;

        public NavigationMenuViewComponent(VolunteerappContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await GetCurrentUser();
            var isAdmin = user?.IsAdmin == true;

            return View(isAdmin);
        }

        private async Task<AppUser?> GetCurrentUser()
        {
            if (User.Identity?.IsAuthenticated != true)
                return null;

            return await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
        }
    }
} 