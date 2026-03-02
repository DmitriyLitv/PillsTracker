namespace PillsTracker.Domain.Entities;

public sealed class Medication
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Dosage { get; set; }
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
}
