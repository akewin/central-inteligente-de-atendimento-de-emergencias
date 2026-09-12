using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Security.Policies;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.SystemResources;

public class GetSystemResourcesForSelect
{
    private readonly ISystemResourceRepository _repository;
    private readonly SystemResourceVisibilityPolicy _visibility;

    public GetSystemResourcesForSelect(
        ISystemResourceRepository repository,
        SystemResourceVisibilityPolicy visibility)
    {
        _repository = repository;
        _visibility = visibility;
    }

    public async Task<IEnumerable<SystemResourceSelectDto>> ExecuteAsync()
    {
        var query = _repository.Query();

        var visibleQuery = _visibility.ApplyToQuery(query);

        var result = await visibleQuery
            .OrderBy(r => r.ExhibitionName)
            .Select(r => new SystemResourceSelectDto
            {
                Id = r.Id,
                ExhibitionName = r.ExhibitionName
            })
            .ToListAsync();

        return result;
    }
}
