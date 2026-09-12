using Api.Security.Jwt;
using Api.Security.Permissions;
using Api.Models;

namespace Api.Security.Policies;

public class SystemResourceVisibilityPolicy
{
    private readonly CurrentUserContext _currentUser;

    public SystemResourceVisibilityPolicy(CurrentUserContext currentUser)
    {
        _currentUser = currentUser;
    }

    public IQueryable<SystemResource> ApplyToQuery(
        IQueryable<SystemResource> query)
    {
        if (!_currentUser.IsAuthenticated)
            return query.Where(_ => false);

        if (_currentUser.IsRoot())
            return query;

        // Remove ROOT da listagem para não-root
        return query.Where(r => r.Id != BasePermissions.ROOT);
    }

    public bool CanAccess(SystemResource resource)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        if (_currentUser.IsRoot())
            return true;

        return resource.Id != BasePermissions.ROOT;
    }
}
