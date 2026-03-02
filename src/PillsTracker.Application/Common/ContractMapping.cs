using PillsTracker.Contracts.Events;
using PillsTracker.Contracts.Medication;
using PillsTracker.Contracts.Plans;
using PillsTracker.Contracts.TimeAnchors;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Application.Common;

internal static class ContractMapping
{
    public static MedicationDto ToDto(this Medication medication) =>
        new(medication.Id, medication.Name, medication.Unit.ToString(), medication.Form, medication.Notes, medication.OwnerUserId is null);

    public static TimeAnchorDto ToDto(this TimeAnchor anchor) =>
        new(anchor.Id, anchor.Key, anchor.Time.ToString("HH:mm"), anchor.OwnerUserId is null);

    public static IntakeTimeSlotDto ToDto(this IntakeTimeSlot slot) =>
        new(slot.Id, slot.Kind.ToString(), slot.FixedTime?.ToString("HH:mm"), slot.AnchorKey, slot.Instruction);

    public static PlanDto ToDto(this IntakePlan plan, IReadOnlyList<IntakeTimeSlot> slots) =>
        new(
            plan.Id,
            plan.MedicationId,
            plan.DoseAmount,
            plan.StartDateUtcBase.ToString("yyyy-MM-dd"),
            plan.EndDateUtcBase.ToString("yyyy-MM-dd"),
            plan.Status.ToString(),
            plan.Revision,
            slots.Select(ToDto).ToList());

    public static ReminderEventDto ToDto(this ReminderEvent evt, bool treatPlannedPastAsFired = false, DateTimeOffset? nowUtc = null)
    {
        var status = evt.Status;
        if (treatPlannedPastAsFired && nowUtc.HasValue && status == ReminderEventStatus.Planned && nowUtc.Value >= evt.ScheduledAtUtc)
        {
            status = ReminderEventStatus.Fired;
        }

        return new ReminderEventDto(
            evt.Id,
            evt.PlanId,
            evt.ScheduledAtUtc.ToString("O"),
            status.ToString(),
            evt.ActionAtUtc?.ToString("O"),
            evt.SnoozedUntilUtc?.ToString("O"),
            evt.SnoozeCount);
    }
}
