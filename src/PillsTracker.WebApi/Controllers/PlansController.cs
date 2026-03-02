using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.UseCases.Plans;
using PillsTracker.Contracts.Plans;

namespace PillsTracker.WebApi.Controllers;

public sealed record SetStatusRequest(string Status);

[ApiController]
[Authorize]
[Route("api/plans")]
public sealed class PlansController(IDispatcher dispatcher) : ControllerBase
{
    [HttpPost]
    public Task<PlanDto> Post([FromBody] CreatePlanRequest request, CancellationToken ct)
        => dispatcher.Send(new CreatePlanCommand(request.MedicationId, request.DoseAmount, request.StartDate, request.DurationDays, request.Slots), ct);

    [HttpPut("{id:guid}")]
    public Task<PlanDto> Put(Guid id, [FromBody] UpdatePlanRequest request, CancellationToken ct)
        => dispatcher.Send(new UpdatePlanCommand(id, request.DoseAmount, request.StartDate, request.DurationDays, request.Status, request.Slots), ct);

    [HttpGet]
    public Task<List<PlanDto>> Get(CancellationToken ct)
        => dispatcher.Query(new GetPlansQuery(), ct);

    [HttpPut("{id:guid}/status")]
    public Task<PlanDto> SetStatus(Guid id, [FromBody] SetStatusRequest request, CancellationToken ct)
        => dispatcher.Send(new SetPlanStatusCommand(id, request.Status), ct);
}
