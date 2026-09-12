using Api.Dtos;
using Api.Services.Hospitals;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/hospital-receptions")]
public sealed class HospitalReceptionsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromServices] HospitalReception service, [FromQuery] string scope = "active", CancellationToken ct = default)
        => Ok(await service.ListAsync(scope, ct));
    [HttpPost("{id:int}/actions")]
    public async Task<IActionResult> Act(int id, [FromBody] HospitalReceptionActionDto dto, [FromServices] HospitalReception service, CancellationToken ct)
        => Ok(await service.ActAsync(id, dto, ct));
    [HttpGet("hospital")]
    public async Task<IActionResult> Hospital([FromServices] HospitalReception service, CancellationToken ct)
        => Ok(await service.GetHospitalAsync(ct));
    [HttpPut("wards/{wardId:int}")]
    public async Task<IActionResult> UpdateWard(int wardId, [FromBody] HospitalWardSaveDto dto, [FromServices] HospitalReception service, CancellationToken ct)
        => Ok(await service.UpdateWardAsync(wardId, dto, ct));
}
