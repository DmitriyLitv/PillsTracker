using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PillsTracker.Application.Abstractions.Services;
using PillsTracker.Application.Abstractions.Time;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;
using PillsTracker.Infrastructure.Persistence;

namespace PillsTracker.Infrastructure.Services;

public sealed class ReminderEventGenerator(
    PillsTrackerDbContext dbContext,
    ITimeZoneResolver timeZoneResolver,
    ILogger<ReminderEventGenerator> logger) : IReminderEventGenerator
{
    public async Task EnsureWindowAsync(Guid userId, DateTimeOffset nowUtc, int daysWindow, CancellationToken ct)
    {
        var plans = await dbContext.IntakePlans
            .Where(x => x.UserId == userId)
            .ToListAsync(ct);

        var userTimeZoneId = await dbContext.Users.Where(x => x.Id == userId).Select(x => x.LastKnownTimeZoneId).FirstOrDefaultAsync(ct);
        var timeZone = timeZoneResolver.Resolve(userTimeZoneId);
        var baseDateUtc = nowUtc.UtcDateTime.Date;

        var userAnchors = await dbContext.TimeAnchors.Where(x => x.OwnerUserId == userId).ToDictionaryAsync(x => x.Key, x => x.Time, ct);
        var systemAnchors = await dbContext.TimeAnchors.Where(x => x.OwnerUserId == null).ToDictionaryAsync(x => x.Key, x => x.Time, ct);

        foreach (var plan in plans)
        {
            if (plan.Status != PlanStatus.Active)
                continue;

            var nowLocalDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(nowUtc, timeZone).DateTime);
            if (plan.EndDateUtcBase < nowLocalDate)
            {
                plan.ChangeStatus(PlanStatus.Completed, nowUtc);
                dbContext.IntakePlans.Update(plan);
                continue;
            }

            var staleFuture = await dbContext.ReminderEvents
                .Where(x => x.PlanId == plan.Id && x.PlanRevision < plan.Revision && x.ScheduledAtUtc >= nowUtc)
                .ToListAsync(ct);

            foreach (var oldEvent in staleFuture)
            {
                oldEvent.SetStatus(ReminderEventStatus.Cancelled, nowUtc);
            }

            var slots = await dbContext.IntakeTimeSlots.Where(x => x.PlanId == plan.Id).ToListAsync(ct);
            for (var offset = 0; offset < daysWindow; offset++)
            {
                var dayStartUtc = baseDateUtc.AddDays(offset);
                var localDate = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(dayStartUtc, timeZone));
                if (localDate < plan.StartDateUtcBase || localDate > plan.EndDateUtcBase)
                    continue;

                foreach (var slot in slots)
                {
                    var localTime = ResolveSlotTime(slot, userAnchors, systemAnchors);
                    if (localTime is null)
                    {
                        logger.LogWarning("Anchor not found for slot {SlotId}.", slot.Id);
                        continue;
                    }

                    var localDateTime = localDate.ToDateTime(localTime.Value, DateTimeKind.Unspecified);
                    var scheduledUtc = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);

                    var exists = await dbContext.ReminderEvents.AnyAsync(x =>
                        x.PlanId == plan.Id &&
                        x.PlanRevision == plan.Revision &&
                        x.ScheduledAtUtc == new DateTimeOffset(scheduledUtc), ct);

                    if (!exists)
                    {
                        await dbContext.ReminderEvents.AddAsync(new ReminderEvent(
                            Guid.NewGuid(), userId, plan.Id, plan.Revision, new DateTimeOffset(scheduledUtc), ReminderEventStatus.Planned), ct);
                    }
                }
            }
        }

        await dbContext.SaveChangesAsync(ct);
    }

    private static TimeOnly? ResolveSlotTime(IntakeTimeSlot slot, IReadOnlyDictionary<string, TimeOnly> userAnchors, IReadOnlyDictionary<string, TimeOnly> systemAnchors)
    {
        if (slot.Kind == SlotKind.FixedTime)
            return slot.FixedTime;

        if (slot.AnchorKey is null)
            return null;

        if (userAnchors.TryGetValue(slot.AnchorKey, out var userTime))
            return userTime;

        return systemAnchors.TryGetValue(slot.AnchorKey, out var systemTime) ? systemTime : null;
    }
}
