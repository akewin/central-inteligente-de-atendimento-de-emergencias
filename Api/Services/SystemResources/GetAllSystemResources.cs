using Api.Dtos;
using Api.Helpers;
using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;

namespace Api.Services.SystemResources;

public class GetAllSystemResources
{
    private readonly ISystemResourceRepository _repository;

    public GetAllSystemResources(ISystemResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<SystemResourceReadDto>> ExecuteAsync(
      int page = 1,
      int pageSize = 10)
    {
        Guard.AgainstNonPositiveInt(page);
        Guard.AgainstNonPositiveInt(pageSize);

        var pagedResources = await _repository.GetAllAsync(page, pageSize);

        var data = pagedResources.Data.Select(r => new SystemResourceReadDto
        {
            Id = r.Id,
            Name = r.Name,
            ExhibitionName = r.ExhibitionName
        }).ToList();

        return new PagedResult<SystemResourceReadDto>(
          pagedResources.TotalItems,
          pagedResources.Page,
          pagedResources.PageSize,
          data
        );
    }
}
