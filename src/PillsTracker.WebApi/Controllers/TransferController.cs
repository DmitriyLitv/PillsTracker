using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.UseCases.Transfer;
using PillsTracker.Contracts.Transfer;

namespace PillsTracker.WebApi.Controllers;

[ApiController]
[Authorize]
public sealed class TransferController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet("api/export")]
    public Task<ExportDto> Export(CancellationToken ct)
        => dispatcher.Query(new ExportQuery(), ct);

    [HttpPost("api/import")]
    public async Task<IActionResult> Import([FromBody] ImportRequest request, CancellationToken ct)
    {
        await dispatcher.Send(new ImportCommand(request.Data, request.Mode), ct);
        return Ok();
    }
}
