using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVCAlumnos.Models;

namespace MVCAlumnos.Services
{
    public class PasswordChangeFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public PasswordChangeFilter(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            if (user != null && user.DebeCambiarPassword)
            {
                var currentPath = context.HttpContext.Request.Path.Value;
                if (!currentPath.Contains("/Manage/ChangePassword") &&
                    !currentPath.Contains("/Account/Logout") &&
                    !currentPath.Contains("/Account/Login"))
                {
                    context.Result = new RedirectToPageResult("Identity/Account/Manage/ChangePassword", new { area = "Identity" });
                    return;
                }
            }

            await next();
        }
    }
}