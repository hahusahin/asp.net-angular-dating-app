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
            if (context.HttpContext.User.Identity?.IsAuthenticated != true) return;
            // Get the username and repo from the context
            int userId = resultContext.HttpContext.User.GetUserId();
            var unitOfWork = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            // fetch user from DB
            var user = await unitOfWork.UserRepository.GetUserByIdAsync(userId);
            if (user == null) return;
            // Update the current user's last active timestamp in the DB
            user.LastActive = DateTime.UtcNow;
            await unitOfWork.Complete();
        }
    }
}
