using Api.Data;
using Api.Interfaces.Repositories;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApiDbContext _context;

    public RefreshTokenRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }

    public Task<RefreshToken?> FindByHashAsync(string hash)
    {
        // when we retrieve the token we need the user's permissions too;
        // otherwise the JWT generated during refresh will be missing claims
        // and endpoints guarded by permissions will return 403.
        return _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.AccessPermissions)
                    .ThenInclude(ap => ap.SystemResource)
            .Include(rt => rt.User)
                .ThenInclude(u => u.Unit)
                    .ThenInclude(unit => unit!.EmergencyService)
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash);
    }

    public Task RevokeAsync(RefreshToken token, DateTime when)
    {
        token.IsRevoked = true;
        token.RevokedAt = when;
        _context.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(int userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();

        if (tokens.Any())
        {
            foreach (var rt in tokens)
            {
                rt.IsRevoked = true;
                rt.RevokedAt = DateTime.UtcNow;
            }

            _context.RefreshTokens.UpdateRange(tokens);
        }
    }
}

