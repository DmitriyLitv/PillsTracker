using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Tests.Unit.Fakes;

internal sealed class FakePlanRepository : IIntakePlanRepository
{
    public readonly List<IntakePlan> Plans = [];
    public readonly Dictionary<Guid, List<IntakeTimeSlot>> Slots = new();

    public Task<IntakePlan?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Plans.FirstOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<IntakePlan>> GetByUserAsync(Guid userId, CancellationToken ct) => Task.FromResult<IReadOnlyList<IntakePlan>>(Plans.Where(x => x.UserId == userId).ToList());
    public Task<IReadOnlyList<IntakePlan>> GetActiveByUserAsync(Guid userId, CancellationToken ct) => Task.FromResult<IReadOnlyList<IntakePlan>>(Plans.Where(x => x.UserId == userId && x.Status == PlanStatus.Active).ToList());
    public Task<IReadOnlyList<IntakeTimeSlot>> GetSlotsByPlanIdAsync(Guid planId, CancellationToken ct) => Task.FromResult<IReadOnlyList<IntakeTimeSlot>>(Slots.GetValueOrDefault(planId, []).ToList());
    public Task AddAsync(IntakePlan plan, IReadOnlyCollection<IntakeTimeSlot> slots, CancellationToken ct) { Plans.Add(plan); Slots[plan.Id] = slots.ToList(); return Task.CompletedTask; }
    public void Update(IntakePlan plan) { }
    public Task ReplaceSlotsAsync(Guid planId, IReadOnlyCollection<IntakeTimeSlot> slots, CancellationToken ct) { Slots[planId] = slots.ToList(); return Task.CompletedTask; }
}

internal sealed class FakeReminderRepository : IReminderEventRepository
{
    public readonly List<ReminderEvent> Events = [];

    public Task<ReminderEvent?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Events.FirstOrDefault(x => x.Id == id));
    public Task<IReadOnlyList<ReminderEvent>> GetByUserAndRangeAsync(Guid userId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<ReminderEvent>>(Events.Where(x => x.UserId == userId && x.ScheduledAtUtc >= fromUtc && x.ScheduledAtUtc <= toUtc).ToList());
    public Task<IReadOnlyList<ReminderEvent>> GetFutureByPlanAndOlderRevisionAsync(Guid planId, int revision, DateTimeOffset fromUtc, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<ReminderEvent>>(Events.Where(x => x.PlanId == planId && x.PlanRevision < revision && x.ScheduledAtUtc >= fromUtc).ToList());
    public Task AddAsync(ReminderEvent reminderEvent, CancellationToken ct) { Events.Add(reminderEvent); return Task.CompletedTask; }
    public void Update(ReminderEvent reminderEvent) { }
}

internal sealed class FakeAnchorRepository : ITimeAnchorRepository
{
    public readonly List<TimeAnchor> Anchors = [];

    public Task<TimeAnchor?> GetByIdAsync(Guid id, CancellationToken ct) => Task.FromResult(Anchors.FirstOrDefault(x => x.Id == id));
    public Task<TimeAnchor?> GetByKeyAndOwnerAsync(string key, Guid ownerUserId, CancellationToken ct) => Task.FromResult(Anchors.FirstOrDefault(x => x.OwnerUserId == ownerUserId && x.Key == key));
    public Task<IReadOnlyList<TimeAnchor>> GetSystemAndByOwnerAsync(Guid ownerUserId, CancellationToken ct) => Task.FromResult<IReadOnlyList<TimeAnchor>>(Anchors.Where(x => x.OwnerUserId == null || x.OwnerUserId == ownerUserId).ToList());
    public Task<IReadOnlyList<TimeAnchor>> GetByOwnerAsync(Guid ownerUserId, CancellationToken ct) => Task.FromResult<IReadOnlyList<TimeAnchor>>(Anchors.Where(x => x.OwnerUserId == ownerUserId).ToList());
    public Task AddAsync(TimeAnchor anchor, CancellationToken ct) { Anchors.Add(anchor); return Task.CompletedTask; }
    public void Update(TimeAnchor anchor) { }
    public void Delete(TimeAnchor anchor) => Anchors.Remove(anchor);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }
    public Task SaveChangesAsync(CancellationToken ct) { SaveChangesCalls++; return Task.CompletedTask; }
}
