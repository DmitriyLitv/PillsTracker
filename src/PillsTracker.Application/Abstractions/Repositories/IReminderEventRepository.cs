using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.Abstractions.Repositories;

public interface IReminderEventRepository
{
    Task<ReminderEvent?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<ReminderEvent>> GetByUserAndRangeAsync(Guid userId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct);
    Task<IReadOnlyList<ReminderEvent>> GetFutureByPlanAndOlderRevisionAsync(Guid planId, int revision, DateTimeOffset fromUtc, CancellationToken ct);
    Task AddAsync(ReminderEvent reminderEvent, CancellationToken ct);
    void Update(ReminderEvent reminderEvent);
}
