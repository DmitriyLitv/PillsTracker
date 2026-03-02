using PillsTracker.Domain.Enums;

namespace PillsTracker.Domain.Entities;

public sealed class ReminderEvent
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PlanId { get; private set; }
    public int PlanRevision { get; private set; }
    public DateTimeOffset ScheduledAtUtc { get; private set; }
    public ReminderEventStatus Status { get; private set; }
    public DateTimeOffset? ActionAtUtc { get; private set; }
    public DateTimeOffset? SnoozedUntilUtc { get; private set; }
    public int SnoozeCount { get; private set; }

    public ReminderEvent(
        Guid id,
        Guid userId,
        Guid planId,
        int planRevision,
        DateTimeOffset scheduledAtUtc,
        ReminderEventStatus status,
        DateTimeOffset? actionAtUtc = null,
        DateTimeOffset? snoozedUntilUtc = null,
        int snoozeCount = 0)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId == Guid.Empty ? throw new ArgumentException("UserId cannot be empty.", nameof(userId)) : userId;
        PlanId = planId == Guid.Empty ? throw new ArgumentException("PlanId cannot be empty.", nameof(planId)) : planId;
        PlanRevision = planRevision < 0 ? throw new ArgumentOutOfRangeException(nameof(planRevision)) : planRevision;
        ScheduledAtUtc = scheduledAtUtc;
        Status = status;
        ActionAtUtc = actionAtUtc;
        SnoozedUntilUtc = snoozedUntilUtc;
        SnoozeCount = snoozeCount < 0 ? throw new ArgumentOutOfRangeException(nameof(snoozeCount)) : snoozeCount;
    }

    public void SetStatus(ReminderEventStatus status, DateTimeOffset? actionAtUtc = null)
    {
        Status = status;
        ActionAtUtc = actionAtUtc;
    }

    public void Snooze(DateTimeOffset untilUtc)
    {
        Status = ReminderEventStatus.Snoozed;
        SnoozedUntilUtc = untilUtc;
        SnoozeCount++;
    }
}
