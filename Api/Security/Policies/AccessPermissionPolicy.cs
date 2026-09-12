using Api.Security.Jwt;
using Api.Middlewares;
using Api.Security.Permissions;

namespace Api.Security.Policies;

public class AccessPermissionPolicy
{
    private readonly CurrentUserContext _currentUser;

    public AccessPermissionPolicy(CurrentUserContext currentUser)
    {
        _currentUser = currentUser;
    }

    public void EnsureCanGrant(IEnumerable<int> permissionIds)
    {
        if (!_currentUser.IsAuthenticated)
            throw new AppException(
                "Usuário não autenticado.",
                StatusCodes.Status401Unauthorized
            );

        if (permissionIds.Contains(BasePermissions.ROOT)
            && !_currentUser.IsRoot())
        {
            throw new AppException(
                "Apenas usuário ROOT pode conceder permissão ROOT.",
                StatusCodes.Status403Forbidden
            );
        }
    }
}
