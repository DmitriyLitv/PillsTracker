namespace PillsTracker.Domain.Enums;

public enum ReminderEventStatus
{
    Planned = 1,
    Fired,
    Taken,
    Skipped,
    Snoozed,
    Cancelled
}
