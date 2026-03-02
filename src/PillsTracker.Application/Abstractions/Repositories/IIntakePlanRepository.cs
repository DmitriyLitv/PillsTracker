using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.Abstractions.Repositories;

public interface IIntakePlanRepository
{
    Task<IntakePlan?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<IntakePlan>> GetByUserAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<IntakePlan>> GetActiveByUserAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<IntakeTimeSlot>> GetSlotsByPlanIdAsync(Guid planId, CancellationToken ct);
    Task AddAsync(IntakePlan plan, IReadOnlyCollection<IntakeTimeSlot> slots, CancellationToken ct);
    void Update(IntakePlan plan);
    Task ReplaceSlotsAsync(Guid planId, IReadOnlyCollection<IntakeTimeSlot> slots, CancellationToken ct);
}
