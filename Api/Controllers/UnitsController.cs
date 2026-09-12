using Api.Dtos;
using Api.Services.Units;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/units")]
public sealed class UnitsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] GetUnits service,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? emergencyService = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
        => Ok(await service.ExecuteAsync(page, pageSize, search, emergencyService, status, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, [FromServices] GetUnitById service, CancellationToken cancellationToken)
        => Ok(await service.ExecuteAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UnitCreateDto? dto, [FromServices] CreateUnit service, CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UnitUpdateDto? dto, [FromServices] UpdateUnit service, CancellationToken cancellationToken)
        => Ok(await service.ExecuteAsync(id, dto, cancellationToken));
}
