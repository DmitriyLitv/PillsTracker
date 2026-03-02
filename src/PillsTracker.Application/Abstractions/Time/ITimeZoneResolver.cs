namespace PillsTracker.Application.Abstractions.Time;

public interface ITimeZoneResolver
{
    TimeZoneInfo Resolve(string? timeZoneId);
}
