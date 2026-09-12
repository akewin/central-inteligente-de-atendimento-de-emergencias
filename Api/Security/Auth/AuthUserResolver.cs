using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Security.Auth;

public class AuthUserResolver
{
    private readonly ApiDbContext _context;

    public AuthUserResolver(ApiDbContext context)
    {
        _context = context;
    }

    public Task<User?> FindByIdentifierAsync(string identifier)
    {
        return _context.Users
            .Include(u => u.AccessPermissions)
                .ThenInclude(ap => ap.SystemResource)
            .Include(u => u.Unit)
                .ThenInclude(unit => unit!.EmergencyService)
            .Include(u => u.Hospital)
            .FirstOrDefaultAsync(u =>
                u.Email == identifier || u.Username == identifier
            );
    }

    public Task<User?> FindByExternalClaimsAsync(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var username =
            principal.FindFirstValue("login") ??
            principal.FindFirstValue(ClaimTypes.Name);

        if (email == null && username == null)
            return Task.FromResult<User?>(null);

        return _context.Users
            .Include(u => u.AccessPermissions)
                .ThenInclude(ap => ap.SystemResource)
            .Include(u => u.Unit)
                .ThenInclude(unit => unit!.EmergencyService)
            .Include(u => u.Hospital)
            .FirstOrDefaultAsync(u =>
                u.Email == email || u.Username == username
            );
    }
}
