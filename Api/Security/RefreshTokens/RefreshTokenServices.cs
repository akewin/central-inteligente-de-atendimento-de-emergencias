using Api.Interfaces.Repositories;
using Api.Models;
using System;
using System.Threading.Tasks;

namespace Api.Security.RefreshTokens;

public class RefreshTokenServices
{
    private readonly IRefreshTokenRepository _repository;
    private readonly Api.Data.ApiDbContext _context;

    public RefreshTokenServices(IRefreshTokenRepository repository, Api.Data.ApiDbContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<(string rawToken, RefreshToken entity)> GenerateAsync(User user)
    {
        var raw = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var hash = HashToken.Compute(raw);

        var entity = new RefreshToken
        {
            TokenHash = hash,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UserId = user.Id
        };

        await _repository.AddAsync(entity);
        await _context.SaveChangesAsync();
        return (raw, entity);
    }

    public async Task<RefreshToken?> ValidateAsync(string rawToken)
    {
        var hash = HashToken.Compute(rawToken);
        var token = await _repository.FindByHashAsync(hash);

        if (token == null || token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
            return null;

        return token;
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        await _repository.RevokeAsync(token, DateTime.UtcNow);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(int userId)
    {
        await _repository.RevokeAllForUserAsync(userId);
        await _context.SaveChangesAsync();
    }

    public async Task<(string rawToken, RefreshToken newEntity)> RotateAsync(RefreshToken oldToken)
    {
        await _repository.RevokeAsync(oldToken, DateTime.UtcNow);
        var user = oldToken.User ?? throw new InvalidOperationException("User must be loaded");
        return await GenerateAsync(user);
    }
}

