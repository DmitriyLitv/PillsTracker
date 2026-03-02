using PillsTracker.Application.Abstractions.Context;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Contracts.TimeAnchors;
using PillsTracker.Domain.Entities;

namespace PillsTracker.Application.UseCases.Anchors;

public sealed record UpsertTimeAnchorCommand(Guid? Id, string Key, string Time) : ICommand<TimeAnchorDto>;
public sealed record GetAnchorsQuery : IQuery<List<TimeAnchorDto>>;

public sealed class UpsertTimeAnchorHandler(
    ITimeAnchorRepository repository,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork) : ICommandHandler<UpsertTimeAnchorCommand, TimeAnchorDto>
{
    public async Task<TimeAnchorDto> Handle(UpsertTimeAnchorCommand command, CancellationToken ct)
    {
        var time = TimeOnly.ParseExact(command.Time, "HH:mm");

        if (command.Id.HasValue)
        {
            var existing = await repository.GetByIdAsync(command.Id.Value, ct) ?? throw new InvalidOperationException("Anchor not found.");
            if (existing.OwnerUserId != currentUser.UserId) throw new InvalidOperationException("Only user anchors can be changed.");

            existing.Update(command.Key, time);
            repository.Update(existing);
            await unitOfWork.SaveChangesAsync(ct);
            return new TimeAnchorDto(existing.Id, existing.Key, existing.Time.ToString("HH:mm"), false);
        }

        var byKey = await repository.GetByKeyAndOwnerAsync(command.Key, currentUser.UserId, ct);
        if (byKey is null)
        {
            byKey = new TimeAnchor(Guid.NewGuid(), command.Key, time, currentUser.UserId);
            await repository.AddAsync(byKey, ct);
        }
        else
        {
            byKey.Update(command.Key, time);
            repository.Update(byKey);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new TimeAnchorDto(byKey.Id, byKey.Key, byKey.Time.ToString("HH:mm"), false);
    }
}

public sealed class GetAnchorsHandler(
    ITimeAnchorRepository repository,
    ICurrentUser currentUser) : IQueryHandler<GetAnchorsQuery, List<TimeAnchorDto>>
{
    public async Task<List<TimeAnchorDto>> Handle(GetAnchorsQuery query, CancellationToken ct)
    {
        var anchors = await repository.GetSystemAndByOwnerAsync(currentUser.UserId, ct);
        return anchors.Select(x => new TimeAnchorDto(x.Id, x.Key, x.Time.ToString("HH:mm"), x.OwnerUserId is null)).ToList();
    }
}
