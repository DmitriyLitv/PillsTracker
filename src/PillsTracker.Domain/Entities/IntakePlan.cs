using PillsTracker.Domain.Enums;

namespace PillsTracker.Domain.Entities;

public sealed class IntakePlan
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MedicationId { get; private set; }
    public decimal DoseAmount { get; private set; }
    public DateOnly StartDateUtcBase { get; private set; }
    public DateOnly EndDateUtcBase { get; private set; }
    public PlanStatus Status { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public IntakePlan(
        Guid id,
        Guid userId,
        Guid medicationId,
        decimal doseAmount,
        DateOnly startDateUtcBase,
        DateOnly endDateUtcBase,
        PlanStatus status,
        int revision,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        if (doseAmount <= 0) throw new ArgumentOutOfRangeException(nameof(doseAmount), "Dose amount must be greater than zero.");
        if (startDateUtcBase > endDateUtcBase) throw new ArgumentException("Start date must be before or equal to end date.");

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = EnsureNotEmpty(userId, nameof(userId));
        MedicationId = EnsureNotEmpty(medicationId, nameof(medicationId));
        DoseAmount = doseAmount;
        StartDateUtcBase = startDateUtcBase;
        EndDateUtcBase = endDateUtcBase;
        Status = status;
        Revision = revision < 0 ? 0 : revision;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void UpdateSchedule(decimal doseAmount, DateOnly startDateUtcBase, DateOnly endDateUtcBase, DateTimeOffset updatedAtUtc)
    {
        if (doseAmount <= 0) throw new ArgumentOutOfRangeException(nameof(doseAmount), "Dose amount must be greater than zero.");
        if (startDateUtcBase > endDateUtcBase) throw new ArgumentException("Start date must be before or equal to end date.");

        DoseAmount = doseAmount;
        StartDateUtcBase = startDateUtcBase;
        EndDateUtcBase = endDateUtcBase;
        Revision++;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void ChangeStatus(PlanStatus status, DateTimeOffset updatedAtUtc)
    {
        Status = status;
        UpdatedAtUtc = updatedAtUtc;
    }

    private static Guid EnsureNotEmpty(Guid value, string paramName)
    {
        if (value == Guid.Empty) throw new ArgumentException("Value cannot be empty.", paramName);
        return value;
    }
}
