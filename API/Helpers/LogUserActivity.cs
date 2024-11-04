using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            // Check whether user is authenticated
            if(context.HttpContext.User.Identity?.IsAuthenticated != true) return;
            // Get the username and repo from the context
            int userId = resultContext.HttpContext.User.GetUserId();
            var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            // fetch user from DB
            var user = await repo.GetUserByIdAsync(userId);
            if (user == null) return;
            // Update the current user's last active timestamp in the DB
            user.LastActive = DateTime.UtcNow;
            await repo.SaveAllAsync();
        }
    }
}
