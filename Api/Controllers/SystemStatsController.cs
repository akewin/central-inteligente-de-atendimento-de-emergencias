using Api.Services.SystemStatsServices;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
  [ApiController]
  [Route("api/stats")]
  public class SystemStatsController : ControllerBase
  {
    private readonly GetSystemStats _getSystemStats;

    public SystemStatsController(GetSystemStats getSystemStats)
    {
      _getSystemStats = getSystemStats;
    }

    // GET: api/stats
    [HttpGet]
    public async Task<IActionResult> Get()
    {
      var stats = await _getSystemStats.ExecuteAsync();
      return Ok(stats);
    }
  }
}