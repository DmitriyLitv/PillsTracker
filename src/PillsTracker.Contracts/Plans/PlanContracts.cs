namespace PillsTracker.Contracts.Plans;

public sealed record IntakeTimeSlotDto(Guid Id, string Kind, string? FixedTime, string? AnchorKey, string? Instruction);

public sealed record CreatePlanRequest(
    Guid MedicationId,
    decimal DoseAmount,
    string StartDate,
    int DurationDays,
    List<CreatePlanSlot> Slots);

public sealed record CreatePlanSlot(string Kind, string? FixedTime, string? AnchorKey, string? Instruction);

public sealed record PlanDto(
    Guid Id,
    Guid MedicationId,
    decimal DoseAmount,
    string StartDate,
    string EndDate,
    string Status,
    int Revision,
    List<IntakeTimeSlotDto> Slots);

public sealed record UpdatePlanRequest(
    decimal DoseAmount,
    string StartDate,
    int DurationDays,
    string Status,
    List<CreatePlanSlot> Slots);
