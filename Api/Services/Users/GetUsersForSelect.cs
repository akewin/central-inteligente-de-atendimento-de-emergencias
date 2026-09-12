using Api.Dtos;
using Api.Interfaces.Repositories;

namespace Api.Services.Users;

public class GetUsersForSelect
{
    private readonly IUserRepository _repository;

    public GetUsersForSelect(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<UserSelectDto>> ExecuteAsync()
    {
        var users = await _repository.GetForSelectAsync();

        return users.Select(u => new UserSelectDto
        {
            Id = u.Id,
            FullName = u.FullName
        });
    }
}
