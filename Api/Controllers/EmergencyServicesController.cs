using Api.Services.EmergencyServices;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/emergency-services")]
public sealed class EmergencyServicesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] GetEmergencyServices service, CancellationToken cancellationToken)
        => Ok(await service.ExecuteAsync(cancellationToken));
}
