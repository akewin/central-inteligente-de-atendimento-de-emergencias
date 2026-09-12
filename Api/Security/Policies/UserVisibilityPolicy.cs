using Api.Models;
using Api.Security.Jwt;
using Api.Security.Permissions;

namespace Api.Security.Policies;

public class UserVisibilityPolicy
{
    private readonly CurrentUserContext _currentUser;

    public UserVisibilityPolicy(CurrentUserContext currentUser)
    {
        _currentUser = currentUser;
    }

    public IQueryable<User> ApplyToQuery(IQueryable<User> query)
    {
        if (!_currentUser.IsAuthenticated)
            return query.Where(_ => false);

        if (_currentUser.IsRoot())
            return query;

        return query.Where(u =>
            !u.AccessPermissions.Any(ap =>
                ap.SystemResourceId == BasePermissions.ROOT
            )
        );
    }

    public bool CanAccess(User target)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        if (_currentUser.IsRoot())
            return true;

        return !target.AccessPermissions.Any(ap =>
            ap.SystemResourceId == BasePermissions.ROOT
        );
    }
}
