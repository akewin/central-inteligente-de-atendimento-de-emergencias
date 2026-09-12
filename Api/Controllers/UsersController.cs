using Api.Dtos;
using Api.Services.Users;
using Api.Services.Users.Orchestrators;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly CreateUserWithAccessGranted _createUser;
    private readonly UpdateUserWithAccessGranted _updateUser;
    private readonly GetAllUsers _getAllUsers;
    private readonly GetUserById _getUserById;
    private readonly GetUsersForSelect _getUsersForSelect;
    private readonly SearchUsers _searchUsers;
    private readonly DeleteUser _deleteUser;

    public UsersController(
        CreateUserWithAccessGranted createUser,
        UpdateUserWithAccessGranted updateUser,
        GetAllUsers getAllUsers,
        GetUserById getUserById,
        GetUsersForSelect getUsersForSelect,
        SearchUsers searchUsers,
        DeleteUser deleteUser)
    {
        _createUser = createUser;
        _updateUser = updateUser;
        _getAllUsers = getAllUsers;
        _getUserById = getUserById;
        _getUsersForSelect = getUsersForSelect;
        _searchUsers = searchUsers;
        _deleteUser = deleteUser;
    }

    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Payload inválido." });

        var created = await _createUser.ExecuteAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            created
        );
    }

    // GET: api/users?page=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var users = await _getAllUsers.ExecuteAsync(page, pageSize);
        return Ok(users);
    }

    // GET: api/users/options
    [HttpGet("options")]
    public async Task<IActionResult> GetOptions()
    {
        var users = await _getUsersForSelect.ExecuteAsync();
        return Ok(users);
    }

    [HttpGet("operational-unit-options")]
    public async Task<IActionResult> GetOperationalUnitOptions(
        [FromServices] GetOperationalUnitOptions service,
        [FromQuery] int? userId,
        CancellationToken cancellationToken)
        => Ok(await service.ExecuteAsync(userId, cancellationToken));

    // GET: api/users/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _getUserById.ExecuteAsync(id);
        return Ok(user);
    }

    // PUT: api/users/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
    {
        if (dto == null)
            return BadRequest(new { message = "Payload inválido." });

        Guard.AgainstMismatchedIds(id, dto.Id);

        var updated = await _updateUser.ExecuteAsync(dto);

        return Ok(updated);
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _deleteUser.ExecuteAsync(id);

        if (!deleted)
            return NotFound(new { message = "Usuário não encontrado." });

        return NoContent();
    }

    // GET: api/users/search?key=abc&page=1&pageSize=10
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string key,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var users = await _searchUsers.ExecuteAsync(key, page, pageSize);
        return Ok(users);
    }
}
