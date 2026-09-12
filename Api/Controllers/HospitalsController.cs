using Api.Dtos;
using Api.Services.Hospitals;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/hospitals")]
public sealed class HospitalsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List([FromServices] ManageHospitals service, [FromQuery] bool includeInactive = false, CancellationToken ct = default)
        => Ok(await service.ListAsync(includeInactive, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] HospitalSaveDto dto, [FromServices] ManageHospitals service, CancellationToken ct)
    { var result = await service.SaveAsync(null, dto, ct); return Created($"/api/hospitals/{result.Id}", result); }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] HospitalSaveDto dto, [FromServices] ManageHospitals service, CancellationToken ct)
        => Ok(await service.SaveAsync(id, dto, ct));

    [HttpPost("{hospitalId:int}/wards")]
    public async Task<IActionResult> CreateWard(int hospitalId, [FromBody] HospitalWardSaveDto dto, [FromServices] ManageHospitals service, CancellationToken ct)
        => StatusCode(201, await service.SaveWardAsync(hospitalId, null, dto, ct));

    [HttpPut("{hospitalId:int}/wards/{wardId:int}")]
    public async Task<IActionResult> UpdateWard(int hospitalId, int wardId, [FromBody] HospitalWardSaveDto dto, [FromServices] ManageHospitals service, CancellationToken ct)
        => Ok(await service.SaveWardAsync(hospitalId, wardId, dto, ct));
}
