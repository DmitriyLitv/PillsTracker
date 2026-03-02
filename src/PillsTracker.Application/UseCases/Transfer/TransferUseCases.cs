using PillsTracker.Application.Abstractions.Context;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Application.Common;
using PillsTracker.Contracts.Plans;
using PillsTracker.Contracts.Transfer;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Application.UseCases.Transfer;

public sealed record ExportQuery : IQuery<ExportDto>;
public sealed record ImportCommand(ExportDto Data, string Mode) : ICommand<bool>;

public sealed class ExportHandler(
    IMedicationRepository medicationRepository,
    ITimeAnchorRepository timeAnchorRepository,
    IIntakePlanRepository intakePlanRepository,
    ICurrentUser currentUser) : IQueryHandler<ExportQuery, ExportDto>
{
    public async Task<ExportDto> Handle(ExportQuery query, CancellationToken ct)
    {
        var meds = (await medicationRepository.GetByOwnerAsync(currentUser.UserId, ct)).Select(x => x.ToDto()).ToList();
        var anchors = (await timeAnchorRepository.GetByOwnerAsync(currentUser.UserId, ct)).Select(x => x.ToDto()).ToList();
        var plans = await intakePlanRepository.GetByUserAsync(currentUser.UserId, ct);
        var planDtos = new List<PlanDto>(plans.Count);
        foreach (var p in plans)
        {
            var slots = await intakePlanRepository.GetSlotsByPlanIdAsync(p.Id, ct);
            planDtos.Add(p.ToDto(slots));
        }

        return new ExportDto(meds, anchors, planDtos);
    }
}

public sealed class ImportHandler(
    IMedicationRepository medicationRepository,
    ITimeAnchorRepository timeAnchorRepository,
    IIntakePlanRepository intakePlanRepository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<ImportCommand, bool>
{
    public async Task<bool> Handle(ImportCommand command, CancellationToken ct)
    {
        foreach (var m in command.Data.Medications)
        {
            var medication = new Medication(Guid.NewGuid(), m.Name, Parsing.ParseDoseUnit(m.Unit), m.Form, m.Notes, currentUser.UserId);
            await medicationRepository.AddAsync(medication, ct);
        }

        foreach (var a in command.Data.Anchors)
        {
            var anchor = await timeAnchorRepository.GetByKeyAndOwnerAsync(a.Key, currentUser.UserId, ct);
            var time = TimeOnly.ParseExact(a.Time, "HH:mm");
            if (anchor is null)
            {
                await timeAnchorRepository.AddAsync(new TimeAnchor(Guid.NewGuid(), a.Key, time, currentUser.UserId), ct);
            }
            else
            {
                anchor.Update(a.Key, time);
                timeAnchorRepository.Update(anchor);
            }
        }

        foreach (var planDto in command.Data.Plans)
        {
            var start = DateOnly.ParseExact(planDto.StartDate, "yyyy-MM-dd");
            var end = DateOnly.ParseExact(planDto.EndDate, "yyyy-MM-dd");
            var planId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var plan = new IntakePlan(planId, currentUser.UserId, planDto.MedicationId, planDto.DoseAmount, start, end,
                Enum.TryParse<PlanStatus>(planDto.Status, true, out var st) ? st : PlanStatus.Active, planDto.Revision, now, now);

            var slots = planDto.Slots.Select(s =>
            {
                var kind = Enum.TryParse<SlotKind>(s.Kind, true, out var k) ? k : SlotKind.FixedTime;
                var fixedTime = string.IsNullOrWhiteSpace(s.FixedTime) ? null : TimeOnly.ParseExact(s.FixedTime, "HH:mm");
                return new IntakeTimeSlot(Guid.NewGuid(), planId, kind, fixedTime, s.AnchorKey, s.Instruction);
            }).ToList();

            await intakePlanRepository.AddAsync(plan, slots, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}
