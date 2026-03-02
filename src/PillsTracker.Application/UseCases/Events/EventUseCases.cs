using PillsTracker.Application.Abstractions.Context;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.Abstractions.Repositories;
using PillsTracker.Application.Abstractions.Services;
using PillsTracker.Application.Abstractions.Time;
using PillsTracker.Application.Common;
using PillsTracker.Contracts.Events;
using PillsTracker.Domain.Entities;
using PillsTracker.Domain.Enums;

namespace PillsTracker.Application.UseCases.Events;

public sealed record GetUpcomingEventsQuery(DateTimeOffset? FromUtc = null, DateTimeOffset? ToUtc = null) : IQuery<List<ReminderEventDto>>;
public sealed record TakeEventCommand(Guid EventId, string? TakenAtUtc) : ICommand<ReminderEventDto>;
public sealed record SkipEventCommand(Guid EventId, string? SkippedAtUtc) : ICommand<ReminderEventDto>;
public sealed record SnoozeEventCommand(Guid EventId, int Minutes, string? SnoozedAtUtc) : ICommand<ReminderEventDto>;

public sealed class GetUpcomingEventsHandler(
    IReminderEventRepository repository,
    ICurrentUser currentUser,
    IClock clock,
    IUnitOfWork unitOfWork,
    IReminderEventGenerator generator) : IQueryHandler<GetUpcomingEventsQuery, List<ReminderEventDto>>
{
    public async Task<List<ReminderEventDto>> Handle(GetUpcomingEventsQuery query, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var from = query.FromUtc ?? now;
        var to = query.ToUtc ?? now.AddDays(7);

        await generator.EnsureWindowAsync(currentUser.UserId, now, 7, ct);

        var events = await repository.GetByUserAndRangeAsync(currentUser.UserId, from, to, ct);
        foreach (var evt in events.Where(x => x.Status == ReminderEventStatus.Planned && now >= x.ScheduledAtUtc))
        {
            evt.SetStatus(ReminderEventStatus.Fired);
            repository.Update(evt);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return events.Select(x => x.ToDto(true, now)).ToList();
    }
}

public sealed class TakeEventHandler : EventCommandHandlerBase<TakeEventCommand>
{
    public TakeEventHandler(
        IReminderEventRepository eventRepository,
        IIntakeLogRepository logRepository,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork)
        : base(eventRepository, logRepository, currentUser, clock, unitOfWork)
    {
    }

    protected override Guid GetEventId(TakeEventCommand command) => command.EventId;
    protected override ReminderEventStatus GetStatus() => ReminderEventStatus.Taken;
    protected override IntakeAction GetAction() => IntakeAction.Take;
    protected override DateTimeOffset ResolveMoment(TakeEventCommand command, DateTimeOffset now)
        => Parsing.ParseDateTimeOrNow(command.TakenAtUtc, now);
}

public sealed class SkipEventHandler : EventCommandHandlerBase<SkipEventCommand>
{
    public SkipEventHandler(
        IReminderEventRepository eventRepository,
        IIntakeLogRepository logRepository,
        ICurrentUser currentUser,
        IClock clock,
        IUnitOfWork unitOfWork)
        : base(eventRepository, logRepository, currentUser, clock, unitOfWork)
    {
    }

    protected override Guid GetEventId(SkipEventCommand command) => command.EventId;
    protected override ReminderEventStatus GetStatus() => ReminderEventStatus.Skipped;
    protected override IntakeAction GetAction() => IntakeAction.Skip;
    protected override DateTimeOffset ResolveMoment(SkipEventCommand command, DateTimeOffset now)
        => Parsing.ParseDateTimeOrNow(command.SkippedAtUtc, now);
}

public sealed class SnoozeEventHandler(
    IReminderEventRepository eventRepository,
    IIntakeLogRepository logRepository,
    ICurrentUser currentUser,
    IClock clock,
    IUnitOfWork unitOfWork) : ICommandHandler<SnoozeEventCommand, ReminderEventDto>
{
    public async Task<ReminderEventDto> Handle(SnoozeEventCommand command, CancellationToken ct)
    {
        if (command.Minutes <= 0) throw new ArgumentOutOfRangeException(nameof(command.Minutes));

        var evt = await eventRepository.GetByIdAsync(command.EventId, ct) ?? throw new InvalidOperationException("Event not found.");
        if (evt.UserId != currentUser.UserId) throw new InvalidOperationException("Cannot modify foreign event.");

        var at = Parsing.ParseDateTimeOrNow(command.SnoozedAtUtc, clock.UtcNow);
        evt.Snooze(at.AddMinutes(command.Minutes));
        evt.SetStatus(ReminderEventStatus.Snoozed, at);

        await logRepository.AddAsync(new IntakeLog(Guid.NewGuid(), currentUser.UserId, evt.Id, IntakeAction.Snooze, at), ct);
        eventRepository.Update(evt);
        await unitOfWork.SaveChangesAsync(ct);

        return evt.ToDto();
    }
}

public abstract class EventCommandHandlerBase<TCommand>(
    IReminderEventRepository eventRepository,
    IIntakeLogRepository logRepository,
    ICurrentUser currentUser,
    IClock clock,
    IUnitOfWork unitOfWork)
    : ICommandHandler<TCommand, ReminderEventDto>
    where TCommand : ICommand<ReminderEventDto> 
{
    public async Task<ReminderEventDto> Handle(TCommand command, CancellationToken ct)
    {
        var eventId = GetEventId(command);

        var evt = await eventRepository.GetByIdAsync(eventId, ct)
            ?? throw new InvalidOperationException("Event not found.");

        if (evt.UserId != currentUser.UserId)
            throw new InvalidOperationException("Cannot modify foreign event.");

        var at = ResolveMoment(command, clock.UtcNow);

        evt.SetStatus(GetStatus(), at);

        await logRepository.AddAsync(
            new IntakeLog(Guid.NewGuid(), currentUser.UserId, evt.Id, GetAction(), at),
            ct
        );

        eventRepository.Update(evt);
        await unitOfWork.SaveChangesAsync(ct);

        return evt.ToDto();
    }

    protected abstract Guid GetEventId(TCommand command);
    protected abstract ReminderEventStatus GetStatus();
    protected abstract IntakeAction GetAction();
    protected abstract DateTimeOffset ResolveMoment(TCommand command, DateTimeOffset now);
}
