namespace PillsTracker.Application.Abstractions.Services;

public interface IReminderEventGenerator
{
    Task EnsureWindowAsync(Guid userId, DateTimeOffset nowUtc, int daysWindow, CancellationToken ct);
}
