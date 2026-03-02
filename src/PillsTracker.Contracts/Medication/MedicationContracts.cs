namespace PillsTracker.Contracts.Medication;

public sealed record MedicationDto(Guid Id, string Name, string Unit, string? Form, string? Notes, bool IsGlobal);
public sealed record CreateMedicationRequest(string Name, string Unit, string? Form, string? Notes);
public sealed record UpdateMedicationRequest(string Name, string Unit, string? Form, string? Notes);
