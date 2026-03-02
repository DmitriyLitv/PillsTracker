using Microsoft.AspNetCore.Identity;

namespace PillsTracker.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string? LastKnownTimeZoneId { get; set; }
}
