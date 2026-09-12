using Api.Models;
using System;
using System.Threading.Tasks;

namespace Api.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token);
        Task<RefreshToken?> FindByHashAsync(string hash);
        Task RevokeAsync(RefreshToken token, DateTime when);
        Task RevokeAllForUserAsync(int userId);
    }
}
