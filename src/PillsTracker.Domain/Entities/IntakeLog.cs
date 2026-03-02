using PillsTracker.Domain.Enums;

namespace PillsTracker.Domain.Entities;

public sealed class IntakeLog
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }
    public IntakeAction Action { get; private set; }
    public DateTimeOffset ActionAtUtc { get; private set; }
    public string? Note { get; private set; }

    public IntakeLog(Guid id, Guid userId, Guid eventId, IntakeAction action, DateTimeOffset actionAtUtc, string? note = null)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId == Guid.Empty ? throw new ArgumentException("UserId cannot be empty.", nameof(userId)) : userId;
        EventId = eventId == Guid.Empty ? throw new ArgumentException("EventId cannot be empty.", nameof(eventId)) : eventId;
        Action = action;
        ActionAtUtc = actionAtUtc;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}
