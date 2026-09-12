using Api.Services.Geography;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/addresses")]
public sealed class AddressesController : ControllerBase
{
    [HttpGet("postal-code/{postalCode}")]
    public async Task<IActionResult> GetByPostalCode(
        string postalCode,
        [FromServices] LookupPostalCode service,
        CancellationToken cancellationToken)
        => Ok(await service.ExecuteAsync(postalCode, cancellationToken));
}
