using PillsTracker.Application.Abstractions.Time;

namespace PillsTracker.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
