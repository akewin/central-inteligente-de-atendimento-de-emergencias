using Api.Interfaces.Repositories;
using Api.Models;

namespace Api.Services.AccessPermissions;

public class CreateAccessPermissions
{
    private readonly IAccessPermissionRepository _repository;

    public CreateAccessPermissions(IAccessPermissionRepository repository)
    {
        _repository = repository;
    }

    public async Task ExecuteAsync(int userId, IEnumerable<int> systemResourceIds)
    {
        var permissions = systemResourceIds.Select(resourceId => new AccessPermission
        {
            UserId = userId,
            SystemResourceId = resourceId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await _repository.AddRangeAsync(permissions);
    }
}
