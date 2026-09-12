using Api.Auditing.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/reports")]
public class SystemLogsController : ControllerBase
{
    private readonly GetAllSystemLogs _getAllSystemLogs;
    private readonly GetSystemLogById _getSystemLogById;

    public SystemLogsController(
        GetAllSystemLogs getAllSystemLogs,
        GetSystemLogById getSystemLogById)
    {
        _getAllSystemLogs = getAllSystemLogs;
        _getSystemLogById = getSystemLogById;
    }

    // Exemplos:
    // GET /api/reports?userId=5&page=1&pageSize=20
    // GET /api/reports?action=DELETE
    // GET /api/reports?startDate=2025-01-01&endDate=2025-02-01
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? userId = null,
        [FromQuery] string? action = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _getAllSystemLogs.ExecuteAsync(
            userId,
            action,
            startDate,
            endDate,
            page,
            pageSize
        );

        return Ok(result);
    }

    // GET /api/reports/10
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var log = await _getSystemLogById.ExecuteAsync(id);
        return Ok(log);
    }
}
