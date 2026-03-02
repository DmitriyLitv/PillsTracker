namespace PillsTracker.Application.Abstractions.Context;

public interface ICurrentUser
{
    Guid UserId { get; }
    string? TimeZoneId { get; }
}
