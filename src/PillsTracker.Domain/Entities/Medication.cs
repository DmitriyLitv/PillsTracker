using PillsTracker.Domain.Enums;

namespace PillsTracker.Domain.Entities;

public sealed class Medication
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Form { get; private set; }
    public DoseUnit Unit { get; private set; }
    public string? Notes { get; private set; }
    public Guid? OwnerUserId { get; private set; }

    public Medication(Guid id, string name, DoseUnit unit, string? form = null, string? notes = null, Guid? ownerUserId = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        Name = EnsureRequired(name, nameof(name));
        Unit = unit;
        Form = Normalize(form);
        Notes = Normalize(notes);
        OwnerUserId = ownerUserId;
    }

    public void UpdateDetails(string name, DoseUnit unit, string? form, string? notes)
    {
        Name = EnsureRequired(name, nameof(name));
        Unit = unit;
        Form = Normalize(form);
        Notes = Normalize(notes);
    }

    private static string EnsureRequired(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }

        return value.Trim();
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
