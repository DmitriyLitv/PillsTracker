using Microsoft.EntityFrameworkCore;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;
using PillsTracker.Infrastructure.Persistence;

namespace PillsTracker.Infrastructure.Repositories;

public sealed class IntakePlanRepository(PillsTrackerDbContext dbContext) : IIntakePlanRepository
{
    public Task<IntakePlan?> GetByIdAsync(Guid id, CancellationToken ct)
        => dbContext.IntakePlans.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<IReadOnlyList<IntakePlan>> GetByUserAsync(Guid userId, CancellationToken ct)
        => dbContext.IntakePlans.Where(x => x.UserId == userId).ToListAsync(ct).ContinueWith<IReadOnlyList<IntakePlan>>(t => t.Result, ct);

    public Task<IReadOnlyList<IntakePlan>> GetActiveByUserAsync(Guid userId, CancellationToken ct)
        => dbContext.IntakePlans.Where(x => x.UserId == userId && x.Status == PlanStatus.Active).ToListAsync(ct)
            .ContinueWith<IReadOnlyList<IntakePlan>>(t => t.Result, ct);

    public Task<IReadOnlyList<IntakeTimeSlot>> GetSlotsByPlanIdAsync(Guid planId, CancellationToken ct)
        => dbContext.IntakeTimeSlots.Where(x => x.PlanId == planId).ToListAsync(ct).ContinueWith<IReadOnlyList<IntakeTimeSlot>>(t => t.Result, ct);

    public async Task AddAsync(IntakePlan plan, IReadOnlyCollection<IntakeTimeSlot> slots, CancellationToken ct)
    {
        await dbContext.IntakePlans.AddAsync(plan, ct);
        await dbContext.IntakeTimeSlots.AddRangeAsync(slots, ct);
    }

    public void Update(IntakePlan plan) => dbContext.IntakePlans.Update(plan);

    public async Task ReplaceSlotsAsync(Guid planId, IReadOnlyCollection<IntakeTimeSlot> slots, CancellationToken ct)
    {
        var current = await dbContext.IntakeTimeSlots.Where(x => x.PlanId == planId).ToListAsync(ct);
        dbContext.IntakeTimeSlots.RemoveRange(current);
        await dbContext.IntakeTimeSlots.AddRangeAsync(slots, ct);
    }
}
