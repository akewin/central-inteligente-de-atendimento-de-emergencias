using Api.Helpers.Pagination;
using Api.Models;

namespace Api.Interfaces.Repositories;

public interface ISystemResourceRepository
{
    Task<SystemResource> CreateAsync(SystemResource resource);
    Task<SystemResource> UpdateAsync(SystemResource resource);
    Task<bool> SoftDeleteAsync(int id);

    Task<SystemResource?> GetByIdAsync(int id);
    Task<PagedResult<SystemResource>> GetAllAsync(int page, int pageSize);
    IQueryable<SystemResource> Query();
    Task<PagedResult<SystemResource>> SearchAsync(string key, int page, int pageSize);

    Task<bool> ExistsByNameAsync(string name);
}
