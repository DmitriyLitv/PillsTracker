using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PillsTracker.Application.Abstractions.Messaging;
using PillsTracker.Application.UseCases.Medications;
using PillsTracker.Contracts.Medication;

namespace PillsTracker.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/medications")]
public sealed class MedicationsController(IDispatcher dispatcher) : ControllerBase
{
    [HttpGet]
    public Task<List<MedicationDto>> Get(CancellationToken ct)
        => dispatcher.Query(new GetMyMedicationsQuery(), ct);

    [HttpPost]
    public Task<MedicationDto> Post([FromBody] CreateMedicationRequest request, CancellationToken ct)
        => dispatcher.Send(new CreateMedicationCommand(request.Name, request.Unit, request.Form, request.Notes), ct);

    [HttpPut("{id:guid}")]
    public Task<MedicationDto> Put(Guid id, [FromBody] UpdateMedicationRequest request, CancellationToken ct)
        => dispatcher.Send(new UpdateMedicationCommand(id, request.Name, request.Unit, request.Form, request.Notes), ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await dispatcher.Send(new DeleteMedicationCommand(id), ct);
        return NoContent();
    }
}
