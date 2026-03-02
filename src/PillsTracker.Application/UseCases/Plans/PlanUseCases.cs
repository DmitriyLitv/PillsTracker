using PillsTracker.Application.Abstractions.Context;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Application.Common;
using PillsTracker.Contracts.Plans;
using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.UseCases.Plans;

public sealed record CreatePlanCommand(Guid MedicationId, decimal DoseAmount, string StartDate, int DurationDays, List<CreatePlanSlot> Slots) : ICommand<PlanDto>;
public sealed record UpdatePlanCommand(Guid PlanId, decimal DoseAmount, string StartDate, int DurationDays, string Status, List<CreatePlanSlot> Slots) : ICommand<PlanDto>;
public sealed record SetPlanStatusCommand(Guid PlanId, string Status) : ICommand<PlanDto>;
public sealed record GetPlansQuery : IQuery<List<PlanDto>>;

public sealed class CreatePlanHandler(
    IIntakePlanRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<CreatePlanCommand, PlanDto>
{
    public async Task<PlanDto> Handle(CreatePlanCommand command, CancellationToken ct)
    {
        if (command.DurationDays <= 0) throw new ArgumentOutOfRangeException(nameof(command.DurationDays));

        var startDate = Parsing.ParseDate(command.StartDate);
        var endDate = startDate.AddDays(command.DurationDays - 1);
        var now = DateTimeOffset.UtcNow;
        var planId = Guid.NewGuid();

        var plan = new IntakePlan(planId, currentUser.UserId, command.MedicationId, command.DoseAmount, startDate, endDate,
            Domain.Enums.PlanStatus.Active, 1, now, now);

        var slots = Parsing.CreateSlots(planId, command.Slots);
        await repository.AddAsync(plan, slots, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return plan.ToDto(slots.ToList());
    }
}

public sealed class UpdatePlanHandler(
    IIntakePlanRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdatePlanCommand, PlanDto>
{
    public async Task<PlanDto> Handle(UpdatePlanCommand command, CancellationToken ct)
    {
        if (command.DurationDays <= 0) throw new ArgumentOutOfRangeException(nameof(command.DurationDays));

        var plan = await repository.GetByIdAsync(command.PlanId, ct) ?? throw new InvalidOperationException("Plan not found.");
        if (plan.UserId != currentUser.UserId) throw new InvalidOperationException("Cannot edit foreign plan.");

        var start = Parsing.ParseDate(command.StartDate);
        var end = start.AddDays(command.DurationDays - 1);
        var oldSlots = await repository.GetSlotsByPlanIdAsync(plan.Id, ct);
        var newSlots = Parsing.CreateSlots(plan.Id, command.Slots);

        var isScheduleChanged = plan.DoseAmount != command.DoseAmount || plan.StartDateUtcBase != start || plan.EndDateUtcBase != end || !SlotsEquivalent(oldSlots, newSlots);
        if (isScheduleChanged)
        {
            plan.UpdateSchedule(command.DoseAmount, start, end, DateTimeOffset.UtcNow);
            await repository.ReplaceSlotsAsync(plan.Id, newSlots, ct);
        }

        plan.ChangeStatus(Parsing.ParsePlanStatus(command.Status), DateTimeOffset.UtcNow);
        repository.Update(plan);
        await unitOfWork.SaveChangesAsync(ct);
        return plan.ToDto(newSlots.ToList());
    }

    private static bool SlotsEquivalent(IReadOnlyList<IntakeTimeSlot> oldSlots, IReadOnlyCollection<IntakeTimeSlot> newSlots)
    {
        if (oldSlots.Count != newSlots.Count) return false;

        var oldKey = oldSlots.Select(x => $"{x.Kind}|{x.FixedTime}|{x.AnchorKey}|{x.Instruction}").OrderBy(x => x).ToList();
        var newKey = newSlots.Select(x => $"{x.Kind}|{x.FixedTime}|{x.AnchorKey}|{x.Instruction}").OrderBy(x => x).ToList();
        return oldKey.SequenceEqual(newKey);
    }
}

public sealed class SetPlanStatusHandler(
    IIntakePlanRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<SetPlanStatusCommand, PlanDto>
{
    public async Task<PlanDto> Handle(SetPlanStatusCommand command, CancellationToken ct)
    {
        var plan = await repository.GetByIdAsync(command.PlanId, ct) ?? throw new InvalidOperationException("Plan not found.");
        if (plan.UserId != currentUser.UserId) throw new InvalidOperationException("Cannot edit foreign plan.");

        plan.ChangeStatus(Parsing.ParsePlanStatus(command.Status), DateTimeOffset.UtcNow);
        repository.Update(plan);
        await unitOfWork.SaveChangesAsync(ct);

        var slots = await repository.GetSlotsByPlanIdAsync(plan.Id, ct);
        return plan.ToDto(slots);
    }
}

public sealed class GetPlansHandler(
    IIntakePlanRepository repository,
    ICurrentUser currentUser) : IQueryHandler<GetPlansQuery, List<PlanDto>>
{
    public async Task<List<PlanDto>> Handle(GetPlansQuery query, CancellationToken ct)
    {
        var plans = await repository.GetByUserAsync(currentUser.UserId, ct);
        var result = new List<PlanDto>(plans.Count);

        foreach (var plan in plans)
        {
            var slots = await repository.GetSlotsByPlanIdAsync(plan.Id, ct);
            result.Add(plan.ToDto(slots));
        }

        return result;
    }
}
