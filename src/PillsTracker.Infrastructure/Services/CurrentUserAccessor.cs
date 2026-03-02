using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using PillsTracker.Application.Abstractions.Context;
using PillsTracker.Infrastructure.Identity;

namespace PillsTracker.Infrastructure.Services;

public sealed class CurrentUserAccessor(
    IHttpContextAccessor accessor,
    UserManager<ApplicationUser> userManager) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string? TimeZoneId
    {
        get
        {
            var fromHeader = accessor.HttpContext?.Request.Headers["X-User-TimeZone"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(fromHeader))
                return fromHeader;

            var principal = accessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var user = userManager.GetUserAsync(principal).GetAwaiter().GetResult();
            return user?.LastKnownTimeZoneId;
        }
    }
}
