using PillsTracker.Contracts.Plans;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Application.Common;

internal static class Parsing
{
    public static DoseUnit ParseDoseUnit(string unit)
    {
        if (!Enum.TryParse<DoseUnit>(unit, true, out var parsed))
            throw new ArgumentException($"Unknown dose unit: {unit}", nameof(unit));

        return parsed;
    }

    public static PlanStatus ParsePlanStatus(string status)
    {
        if (!Enum.TryParse<PlanStatus>(status, true, out var parsed))
            throw new ArgumentException($"Unknown plan status: {status}", nameof(status));

        return parsed;
    }

    public static DateOnly ParseDate(string date) => DateOnly.ParseExact(date, "yyyy-MM-dd");

    public static DateTimeOffset ParseDateTimeOrNow(string? value, DateTimeOffset nowUtc)
        => string.IsNullOrWhiteSpace(value) ? nowUtc : DateTimeOffset.Parse(value);

    public static IReadOnlyCollection<IntakeTimeSlot> CreateSlots(Guid planId, IReadOnlyCollection<CreatePlanSlot> slots)
    {
        if (slots.Count == 0) throw new ArgumentException("At least one slot is required.", nameof(slots));

        return slots.Select(slot =>
        {
            if (!Enum.TryParse<SlotKind>(slot.Kind, true, out var kind))
                throw new ArgumentException($"Unknown slot kind: {slot.Kind}", nameof(slots));

            TimeOnly? fixedTime = string.IsNullOrWhiteSpace(slot.FixedTime) ? null : TimeOnly.ParseExact(slot.FixedTime, "HH:mm");
            var anchorKey = string.IsNullOrWhiteSpace(slot.AnchorKey) ? null : slot.AnchorKey.Trim();
            return new IntakeTimeSlot(Guid.NewGuid(), planId, kind, fixedTime, anchorKey, slot.Instruction);
        }).ToList();
    }
}
