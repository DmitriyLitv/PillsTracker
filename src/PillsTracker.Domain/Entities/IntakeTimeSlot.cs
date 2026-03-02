using PillsTracker.Domain.Enums;

namespace PillsTracker.Domain.Entities;

public sealed class IntakeTimeSlot
{
    public Guid Id { get; private set; }
    public Guid PlanId { get; private set; }
    public SlotKind Kind { get; private set; }
    public TimeOnly? FixedTime { get; private set; }
    public string? AnchorKey { get; private set; }
    public string? Instruction { get; private set; }

    public IntakeTimeSlot(
        Guid id,
        Guid planId,
        SlotKind kind,
        TimeOnly? fixedTime,
        string? anchorKey,
        string? instruction = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        PlanId = planId == Guid.Empty ? throw new ArgumentException("PlanId cannot be empty.", nameof(planId)) : planId;
        Instruction = Normalize(instruction);

        ApplyKind(kind, fixedTime, anchorKey);
    }

    public void ChangeSlot(SlotKind kind, TimeOnly? fixedTime, string? anchorKey, string? instruction)
    {
        Instruction = Normalize(instruction);
        ApplyKind(kind, fixedTime, anchorKey);
    }

    private void ApplyKind(SlotKind kind, TimeOnly? fixedTime, string? anchorKey)
    {
        if (kind == SlotKind.FixedTime)
        {
            if (!fixedTime.HasValue) throw new ArgumentException("FixedTime is required for FixedTime slot.", nameof(fixedTime));
            Kind = kind;
            FixedTime = fixedTime;
            AnchorKey = null;
            return;
        }

        var normalizedAnchor = Normalize(anchorKey);
        if (string.IsNullOrWhiteSpace(normalizedAnchor))
        {
            throw new ArgumentException("AnchorKey is required for AnchorKey slot.", nameof(anchorKey));
        }

        Kind = SlotKind.AnchorKey;
        AnchorKey = normalizedAnchor;
        FixedTime = null;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
