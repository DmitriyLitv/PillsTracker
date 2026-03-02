using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Application.Abstractions.Services;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Application.Services;

public sealed class ReminderEventGenerator(
    IIntakePlanRepository planRepository,
    IReminderEventRepository eventRepository,
    ITimeAnchorRepository anchorRepository,
    IUnitOfWork unitOfWork) : IReminderEventGenerator
{
    public async Task EnsureWindowAsync(Guid userId, DateTimeOffset nowUtc, int daysWindow, CancellationToken ct)
    {
        var plans = await planRepository.GetActiveByUserAsync(userId, ct);
        var anchors = await anchorRepository.GetSystemAndByOwnerAsync(userId, ct);
        var anchorMap = anchors
            .GroupBy(x => x.Key)
            .ToDictionary(g => g.Key, g => g.First().Time);

        var from = new DateTimeOffset(nowUtc.UtcDateTime.Date, TimeSpan.Zero);
        var to = from.AddDays(daysWindow).AddTicks(-1);
        var existing = await eventRepository.GetByUserAndRangeAsync(userId, from, to, ct);

        foreach (var plan in plans)
        {
            var stale = await eventRepository.GetFutureByPlanAndOlderRevisionAsync(plan.Id, plan.Revision, nowUtc, ct);
            foreach (var old in stale)
            {
                old.SetStatus(ReminderEventStatus.Cancelled, nowUtc);
                eventRepository.Update(old);
            }

            var slots = await planRepository.GetSlotsByPlanIdAsync(plan.Id, ct);
            for (var day = 0; day < daysWindow; day++)
            {
                var date = DateOnly.FromDateTime(from.AddDays(day).UtcDateTime);
                if (date < plan.StartDateUtcBase || date > plan.EndDateUtcBase)
                    continue;

                foreach (var slot in slots)
                {
                    var time = slot.Kind == SlotKind.FixedTime
                        ? slot.FixedTime
                        : (slot.AnchorKey is not null && anchorMap.TryGetValue(slot.AnchorKey, out var anchorTime) ? anchorTime : null);

                    if (time is null)
                        continue;

                    var scheduled = new DateTimeOffset(date.ToDateTime(time.Value, DateTimeKind.Utc), TimeSpan.Zero);
                    if (existing.Any(x => x.PlanId == plan.Id && x.PlanRevision == plan.Revision && x.ScheduledAtUtc == scheduled && x.Status != ReminderEventStatus.Cancelled))
                        continue;

                    await eventRepository.AddAsync(new ReminderEvent(Guid.NewGuid(), userId, plan.Id, plan.Revision, scheduled, ReminderEventStatus.Planned), ct);
                }
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}
