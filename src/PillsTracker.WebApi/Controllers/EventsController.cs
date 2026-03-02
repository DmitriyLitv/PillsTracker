using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.UseCases.Events;
using PillsTracker.Contracts.Events;

namespace PillsTracker.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/events")]
public sealed class EventsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public Task<List<ReminderEventDto>> Get([FromQuery] DateTimeOffset? fromUtc, [FromQuery] DateTimeOffset? toUtc, CancellationToken ct)
        => dispatcher.Query(new GetUpcomingEventsQuery(fromUtc, toUtc), ct);

    [HttpPut("{id:guid}/take")]
    public Task<ReminderEventDto> Take(Guid id, [FromBody] TakeEventRequest request, CancellationToken ct)
        => dispatcher.Send(new TakeEventCommand(id, request.TakenAtUtc), ct);

    [HttpPut("{id:guid}/skip")]
    public Task<ReminderEventDto> Skip(Guid id, [FromBody] SkipEventRequest request, CancellationToken ct)
        => dispatcher.Send(new SkipEventCommand(id, request.SkippedAtUtc), ct);

    [HttpPut("{id:guid}/snooze")]
    public Task<ReminderEventDto> Snooze(Guid id, [FromBody] SnoozeEventRequest request, CancellationToken ct)
        => dispatcher.Send(new SnoozeEventCommand(id, request.Minutes, request.SnoozedAtUtc), ct);
}
