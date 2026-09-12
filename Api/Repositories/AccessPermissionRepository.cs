using Api.Data;
using Api.Interfaces.Repositories;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class AccessPermissionRepository : IAccessPermissionRepository
{
    private readonly ApiDbContext _context;

    public AccessPermissionRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(IEnumerable<AccessPermission> permissions)
    {
        await _context.AccessPermissions.AddRangeAsync(permissions);
    }

    public async Task RemoveByUserIdAsync(int userId)
    {
        var permissions = await _context.AccessPermissions
            .Where(ap => ap.UserId == userId)
            .ToListAsync();

        if (permissions.Any())
            _context.AccessPermissions.RemoveRange(permissions);
    }
}
