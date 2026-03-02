namespace PillsTracker.Infrastructure.Identity;

public sealed class RefreshTokenEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public bool IsRevoked { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
