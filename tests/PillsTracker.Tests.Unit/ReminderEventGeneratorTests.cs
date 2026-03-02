using PillsTracker.Application.Services;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;
using PillsTracker.Tests.Unit.Fakes;

namespace PillsTracker.Tests.Unit;

public sealed class ReminderEventGeneratorTests
{
    [Fact]
    public async Task FixedTime_TwiceADay_Generates14EventsOn7Days()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var plans = new FakePlanRepository();
        plans.Plans.Add(new IntakePlan(planId, userId, Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 7), PlanStatus.Active, 1, now, now));
        plans.Slots[planId] =
        [
            new IntakeTimeSlot(Guid.NewGuid(), planId, SlotKind.FixedTime, new TimeOnly(9, 0), null),
            new IntakeTimeSlot(Guid.NewGuid(), planId, SlotKind.FixedTime, new TimeOnly(21, 0), null)
        ];

        var reminders = new FakeReminderRepository();
        var sut = new ReminderEventGenerator(plans, reminders, new FakeAnchorRepository(), new FakeUnitOfWork());
        await sut.EnsureWindowAsync(userId, now, 7, CancellationToken.None);

        Assert.Equal(14, reminders.Events.Count);
    }

    [Fact]
    public async Task AnchorKey_UsesAnchorTime()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var plans = new FakePlanRepository();
        plans.Plans.Add(new IntakePlan(planId, userId, Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 1), PlanStatus.Active, 1, now, now));
        plans.Slots[planId] = [new IntakeTimeSlot(Guid.NewGuid(), planId, SlotKind.AnchorKey, null, "Перед сном")];

        var reminders = new FakeReminderRepository();
        var anchors = new FakeAnchorRepository();
        anchors.Anchors.Add(new TimeAnchor(Guid.NewGuid(), "Перед сном", new TimeOnly(22, 30), userId));

        var sut = new ReminderEventGenerator(plans, reminders, anchors, new FakeUnitOfWork());
        await sut.EnsureWindowAsync(userId, now, 7, CancellationToken.None);

        Assert.Single(reminders.Events);
        Assert.Equal(new TimeOnly(22, 30), TimeOnly.FromDateTime(reminders.Events[0].ScheduledAtUtc.UtcDateTime));
    }

    [Fact]
    public async Task RevisionIncrease_CancelsOldFutureAndCreatesNew()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var plans = new FakePlanRepository();
        plans.Plans.Add(new IntakePlan(planId, userId, Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 7), PlanStatus.Active, 2, now, now));
        plans.Slots[planId] = [new IntakeTimeSlot(Guid.NewGuid(), planId, SlotKind.FixedTime, new TimeOnly(9, 0), null)];

        var reminders = new FakeReminderRepository();
        reminders.Events.Add(new ReminderEvent(Guid.NewGuid(), userId, planId, 1, now.AddHours(1), ReminderEventStatus.Planned));

        var sut = new ReminderEventGenerator(plans, reminders, new FakeAnchorRepository(), new FakeUnitOfWork());
        await sut.EnsureWindowAsync(userId, now, 7, CancellationToken.None);

        Assert.Contains(reminders.Events, x => x.PlanRevision == 1 && x.Status == ReminderEventStatus.Cancelled);
        Assert.Contains(reminders.Events, x => x.PlanRevision == 2 && x.Status == ReminderEventStatus.Planned);
    }

    [Fact]
    public async Task PausedPlan_DoesNotGenerate()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        var plans = new FakePlanRepository();
        plans.Plans.Add(new IntakePlan(planId, userId, Guid.NewGuid(), 1, new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 7), PlanStatus.Paused, 1, now, now));
        plans.Slots[planId] = [new IntakeTimeSlot(Guid.NewGuid(), planId, SlotKind.FixedTime, new TimeOnly(9, 0), null)];

        var reminders = new FakeReminderRepository();
        var sut = new ReminderEventGenerator(plans, reminders, new FakeAnchorRepository(), new FakeUnitOfWork());
        await sut.EnsureWindowAsync(userId, now, 7, CancellationToken.None);

        Assert.Empty(reminders.Events);
    }
}
