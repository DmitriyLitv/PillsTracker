using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.UseCases.Anchors;
using PillsTracker.Contracts.TimeAnchors;

namespace PillsTracker.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/anchors")]
public sealed class AnchorsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public Task<List<TimeAnchorDto>> Get(CancellationToken ct)
        => dispatcher.Query(new GetAnchorsQuery(), ct);

    [HttpPut]
    public Task<TimeAnchorDto> Put([FromBody] UpsertTimeAnchorRequest request, CancellationToken ct)
        => dispatcher.Send(new UpsertTimeAnchorCommand(null, request.Key, request.Time), ct);
}
