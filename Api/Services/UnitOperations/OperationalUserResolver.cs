using System.Net;
using Api.Data;
using Api.Middlewares;
using Api.Models;
using Api.Security.Jwt;
using Api.Security.Permissions;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UnitOperations;

public sealed class OperationalUserResolver
{
    private readonly ApiDbContext _context;
    private readonly CurrentUserContext _currentUser;

    public OperationalUserResolver(ApiDbContext context, CurrentUserContext currentUser)
        => (_context, _currentUser) = (context, currentUser);

    public async Task<User> ResolveAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetId()
            ?? throw new AppException("Usuário não autenticado.", (int)HttpStatusCode.Unauthorized);
        var claimedUnitId = _currentUser.GetUnitId();
        var user = await _context.Users.AsNoTracking()
            .Include(item => item.Unit).ThenInclude(unit => unit!.EmergencyService)
            .Include(item => item.AccessPermissions)
            .SingleOrDefaultAsync(item => item.Id == userId && item.Active, cancellationToken)
            ?? throw new AppException("Conta operacional não encontrada ou inativa.", (int)HttpStatusCode.Unauthorized);

        if (!user.UnitId.HasValue || user.Unit is null || claimedUnitId != user.UnitId)
            throw new AppException("A sessão não possui um vínculo válido com uma equipe.", (int)HttpStatusCode.Forbidden);
        if (!user.AccessPermissions.Any(permission => permission.SystemResourceId == BasePermissions.UNIT_OPERATIONS))
            throw new AppException("A conta não possui permissão operacional.", (int)HttpStatusCode.Forbidden);
        return user;
    }
}
