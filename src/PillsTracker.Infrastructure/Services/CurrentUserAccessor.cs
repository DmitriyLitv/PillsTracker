using System.Security.Claims;
using PillsTracker.Application.Abstractions.Context;

namespace PillsTracker.Infrastructure.Services;

public sealed class CurrentUserAccessor(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string? TimeZoneId => accessor.HttpContext?.Request.Headers["X-TimeZone"].FirstOrDefault();
}
