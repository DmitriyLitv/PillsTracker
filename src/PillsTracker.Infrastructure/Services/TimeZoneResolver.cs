using PillsTracker.Application.Abstractions.Time;
using TimeZoneConverter;

namespace PillsTracker.Infrastructure.Services;

public sealed class TimeZoneResolver : ITimeZoneResolver
{
    public TimeZoneInfo Resolve(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            // fallback below
        }

        try
        {
            var windowsId = TZConvert.IanaToWindows(timeZoneId);
            return TimeZoneInfo.FindSystemTimeZoneById(windowsId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }
}
