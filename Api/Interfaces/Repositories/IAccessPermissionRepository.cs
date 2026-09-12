using Api.Models;

namespace Api.Interfaces.Repositories;

public interface IAccessPermissionRepository
{
    Task AddRangeAsync(IEnumerable<AccessPermission> permissions);

    Task RemoveByUserIdAsync(int userId);
}
