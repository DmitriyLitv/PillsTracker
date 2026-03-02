using Microsoft.EntityFrameworkCore;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Domain.Entities;
using PillsTracker.Infrastructure.Persistence;

namespace PillsTracker.Infrastructure.Repositories;

public sealed class ReminderEventRepository(PillsTrackerDbContext dbContext) : IReminderEventRepository
{
    public Task<ReminderEvent?> GetByIdAsync(Guid id, CancellationToken ct)
        => dbContext.ReminderEvents.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<IReadOnlyList<ReminderEvent>> GetByUserAndRangeAsync(Guid userId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct)
        => dbContext.ReminderEvents
            .Where(x => x.UserId == userId && x.ScheduledAtUtc >= fromUtc && x.ScheduledAtUtc <= toUtc)
            .OrderBy(x => x.ScheduledAtUtc)
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<ReminderEvent>>(t => t.Result, ct);

    public Task<IReadOnlyList<ReminderEvent>> GetFutureByPlanAndOlderRevisionAsync(Guid planId, int revision, DateTimeOffset fromUtc, CancellationToken ct)
        => dbContext.ReminderEvents
            .Where(x => x.PlanId == planId && x.PlanRevision < revision && x.ScheduledAtUtc >= fromUtc)
            .ToListAsync(ct)
            .ContinueWith<IReadOnlyList<ReminderEvent>>(t => t.Result, ct);

    public Task AddAsync(ReminderEvent reminderEvent, CancellationToken ct)
        => dbContext.ReminderEvents.AddAsync(reminderEvent, ct).AsTask();

    public void Update(ReminderEvent reminderEvent) => dbContext.ReminderEvents.Update(reminderEvent);
}
