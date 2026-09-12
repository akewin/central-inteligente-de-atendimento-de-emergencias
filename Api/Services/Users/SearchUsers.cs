using Api.Dtos;
using Api.Helpers;
using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;
using Api.Security.Policies;

namespace Api.Services.Users;

public class SearchUsers
{
    private readonly IUserRepository _repository;
    private readonly UserVisibilityPolicy _visibility;

    public SearchUsers(
        IUserRepository repository,
        UserVisibilityPolicy visibility)
    {
        _repository = repository;
        _visibility = visibility;
    }

    public async Task<PagedResult<UserListDto>> ExecuteAsync(
        string key,
        int page = 1,
        int pageSize = 10)
    {
        Guard.AgainstNullOrEmpty(key, nameof(key));
        Guard.AgainstNonPositiveInt(page);
        Guard.AgainstNonPositiveInt(pageSize);

        var query = _repository.SearchQuery(key);

        var visibleQuery = _visibility.ApplyToQuery(query);

        var dtoQuery = visibleQuery.Select(u => new UserListDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            FullName = u.FullName,
            CreatedAt = u.CreatedAt
        });

        return await PagedResult<UserListDto>.CreateAsync(
            dtoQuery,
            page,
            pageSize
        );
    }
}
