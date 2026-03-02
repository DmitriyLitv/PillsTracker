using Microsoft.AspNetCore.Identity;
using PillsTracker.Infrastructure.Identity;

namespace PillsTracker.WebApi.Middleware;

public sealed class UserTimeZoneMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        var headerTz = context.Request.Headers["X-User-TimeZone"].FirstOrDefault();

        if (context.User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(headerTz))
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is not null && user.LastKnownTimeZoneId != headerTz)
            {
                user.LastKnownTimeZoneId = headerTz;
                await userManager.UpdateAsync(user);
            }
        }

        await next(context);
    }
}
