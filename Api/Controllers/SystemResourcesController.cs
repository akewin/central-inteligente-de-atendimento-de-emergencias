using Api.Dtos;
using Api.Services.SystemResources;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/resources")]
public class SystemResourcesController : ControllerBase
{
    private readonly CreateSystemResource _create;
    private readonly GetAllSystemResources _getAll;
    private readonly GetSystemResourceById _getById;
    private readonly UpdateSystemResource _update;
    private readonly DeleteSystemResource _delete;
    private readonly SearchSystemResources _search;
    private readonly GetSystemResourcesForSelect _getForSelect;

    public SystemResourcesController(
        CreateSystemResource create,
        GetAllSystemResources getAll,
        GetSystemResourceById getById,
        UpdateSystemResource update,
        DeleteSystemResource delete,
        SearchSystemResources search,
        GetSystemResourcesForSelect getForSelect)
    {
        _create = create;
        _getAll = getAll;
        _getById = getById;
        _update = update;
        _delete = delete;
        _search = search;
        _getForSelect = getForSelect;
    }

    // POST: api/resources
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SystemResourceCreateDto dto)
    {
        var result = await _create.ExecuteAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // GET: api/resources?page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _getAll.ExecuteAsync(page, pageSize);
        return Ok(result);
    }

    // GET: api/resources/options
    [HttpGet("options")]
    public async Task<IActionResult> GetForSelect()
    {
        var result = await _getForSelect.ExecuteAsync();
        return Ok(result);
    }

    // GET: api/resources/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _getById.ExecuteAsync(id);
        return Ok(result);
    }

    // PUT: api/resources/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] SystemResourceUpdateDto dto)
    {
        Guard.AgainstMismatchedIds(id, dto.Id);
        var result = await _update.ExecuteAsync(id, dto);
        return Ok(result);
    }

    // DELETE: api/resources/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _delete.ExecuteAsync(id);
        return NoContent();
    }

    // GET: api/resources/search?key=abc&page=1&pageSize=10
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string key,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _search.ExecuteAsync(key, page, pageSize);
        return Ok(result);
    }
}
