using Api.Data;
using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class SystemResourceRepository : ISystemResourceRepository
{
    private readonly ApiDbContext _context;

    public SystemResourceRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<SystemResource> CreateAsync(SystemResource resource)
    {
        _context.SystemResources.Add(resource);
        return resource;
    }

    public async Task<SystemResource> UpdateAsync(SystemResource resource)
    {
        _context.SystemResources.Update(resource);
        return resource;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var resource = await _context.SystemResources
          .FirstOrDefaultAsync(r => r.Id == id && r.Active);

        if (resource == null)
            return false;

        resource.Active = false;
        resource.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<SystemResource?> GetByIdAsync(int id)
    {
        return await _context.SystemResources
          .Include(r => r.AccessPermissions)
          .ThenInclude(ap => ap.User)
          .FirstOrDefaultAsync(r => r.Id == id && r.Active);
    }

    public async Task<PagedResult<SystemResource>> GetAllAsync(int page, int pageSize)
    {
        var query = _context.SystemResources
          .AsNoTracking()
          .Where(r => r.Active)
          .OrderBy(r => r.Id);

        return await PagedResult<SystemResource>
          .CreateAsync(query, page, pageSize);
    }

    public IQueryable<SystemResource> Query()
    {
        return _context.SystemResources
            .AsNoTracking()
            .Where(r => r.Active);
    }

    public async Task<PagedResult<SystemResource>> SearchAsync(
      string key,
      int page,
      int pageSize)
    {
        var query = _context.SystemResources
          .AsNoTracking()
          .Where(r =>
            r.Active &&
            (EF.Functions.ILike(r.Name, $"%{key}%") ||
             EF.Functions.ILike(r.ExhibitionName, $"%{key}%")))
          .OrderBy(r => r.Name);

        return await PagedResult<SystemResource>
          .CreateAsync(query, page, pageSize);
    }

    public Task<bool> ExistsByNameAsync(string name)
      => _context.SystemResources
        .AnyAsync(r => r.Name == name && r.Active);
}
