namespace PillsTracker.Contracts.Events;

public sealed record ReminderEventDto(
    Guid Id,
    Guid PlanId,
    string ScheduledAtUtc,
    string Status,
    string? ActionAtUtc,
    string? SnoozedUntilUtc,
    int SnoozeCount);

public sealed record TakeEventRequest(string? TakenAtUtc);
public sealed record SkipEventRequest(string? SkippedAtUtc);
public sealed record SnoozeEventRequest(int Minutes, string? SnoozedAtUtc);
