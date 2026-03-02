using PillsTracker.Application.Abstractions.Context;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Application.Common;
using PillsTracker.Contracts.Medication;
using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.UseCases.Medications;

public sealed record CreateMedicationCommand(string Name, string Unit, string? Form, string? Notes) : ICommand<MedicationDto>;
public sealed record UpdateMedicationCommand(Guid Id, string Name, string Unit, string? Form, string? Notes) : ICommand<MedicationDto>;
public sealed record DeleteMedicationCommand(Guid Id) : ICommand<bool>;
public sealed record GetMyMedicationsQuery : IQuery<List<MedicationDto>>;

public sealed class CreateMedicationHandler(
    IMedicationRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateMedicationCommand, MedicationDto>
{
    public async Task<MedicationDto> Handle(CreateMedicationCommand command, CancellationToken ct)
    {
        var medication = new Medication(Guid.NewGuid(), command.Name, Parsing.ParseDoseUnit(command.Unit), command.Form, command.Notes, currentUser.UserId);
        await repository.AddAsync(medication, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return medication.ToDto();
    }
}

public sealed class UpdateMedicationHandler(
    IMedicationRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateMedicationCommand, MedicationDto>
{
    public async Task<MedicationDto> Handle(UpdateMedicationCommand command, CancellationToken ct)
    {
        var medication = await repository.GetByIdAsync(command.Id, ct) ?? throw new InvalidOperationException("Medication not found.");
        if (medication.OwnerUserId != currentUser.UserId) throw new InvalidOperationException("Cannot edit global or foreign medication.");

        medication.UpdateDetails(command.Name, Parsing.ParseDoseUnit(command.Unit), command.Form, command.Notes);
        repository.Update(medication);
        await unitOfWork.SaveChangesAsync(ct);
        return medication.ToDto();
    }
}

public sealed class DeleteMedicationHandler(
    IMedicationRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteMedicationCommand, bool>
{
    public async Task<bool> Handle(DeleteMedicationCommand command, CancellationToken ct)
    {
        var medication = await repository.GetByIdAsync(command.Id, ct) ?? throw new InvalidOperationException("Medication not found.");
        if (medication.OwnerUserId != currentUser.UserId) throw new InvalidOperationException("Cannot delete global or foreign medication.");

        repository.Delete(medication);
        await unitOfWork.SaveChangesAsync(ct);
        return true;
    }
}

public sealed class GetMyMedicationsHandler(
    IMedicationRepository repository,
    ICurrentUser currentUser) : IQueryHandler<GetMyMedicationsQuery, List<MedicationDto>>
{
    public async Task<List<MedicationDto>> Handle(GetMyMedicationsQuery query, CancellationToken ct)
    {
        var medications = await repository.GetGlobalAndByOwnerAsync(currentUser.UserId, ct);
        return medications.Select(x => x.ToDto()).ToList();
    }
}
