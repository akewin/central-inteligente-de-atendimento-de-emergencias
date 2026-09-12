using Api.Dtos;
using Api.Services.UnitOperations;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/unit-operations/dispatches")]
public sealed class UnitOperationsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] GetMyDispatches service,
        [FromQuery] string scope = "active", [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        => Ok(await service.ExecuteAsync(scope, page, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromServices] GetMyDispatchById service,
        CancellationToken cancellationToken)
        => Ok(await service.ExecuteAsync(id, cancellationToken));

    [HttpPost("{id:int}/transitions")]
    public async Task<IActionResult> Transition(int id, [FromBody] OperationalDispatchTransitionRequestDto? dto,
        [FromServices] TransitionMyDispatch service, CancellationToken cancellationToken)
        => Ok(await service.ExecuteAsync(id, dto, cancellationToken));

    [HttpGet("{id:int}/route")]
    public async Task<IActionResult> GetRoute(int id, [FromServices] GetMyDispatchRoute service,
        CancellationToken cancellationToken)
        => Ok(await service.ExecuteAsync(id, cancellationToken));

    [HttpPost("{id:int}/transport-request")]
    public async Task<IActionResult> RequestTransport(int id, [FromBody] RequestTransportDto dto,
        [FromServices] ManagePatientTransport service, CancellationToken cancellationToken)
        => StatusCode(201, await service.RequestAsync(id, dto, cancellationToken));

    [HttpPut("{id:int}/transport-destination")]
    public async Task<IActionResult> AssignDestination(int id, [FromBody] AssignTransportDestinationDto dto,
        [FromServices] ManagePatientTransport service, CancellationToken cancellationToken)
        => Ok(await service.AssignDestinationAsync(id, dto, cancellationToken));

    [HttpPost("{id:int}/transport-start")]
    public async Task<IActionResult> StartTransport(int id, [FromServices] ManagePatientTransport service, CancellationToken cancellationToken)
        => Ok(await service.StartAsync(id, cancellationToken));

    [HttpPost("{id:int}/complete-on-site")]
    public async Task<IActionResult> CompleteOnSite(int id, [FromServices] ManagePatientTransport service, CancellationToken cancellationToken)
        => Ok(await service.CompleteOnSiteAsync(id, cancellationToken));
}
