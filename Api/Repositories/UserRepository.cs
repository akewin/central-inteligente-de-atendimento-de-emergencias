using Api.Data;
using Api.Interfaces.Repositories;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApiDbContext _context;

    public UserRepository(ApiDbContext context)
    {
        _context = context;
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return user;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;

        user.Active = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.AccessPermissions)
                .ThenInclude(ap => ap.SystemResource)
            .Include(u => u.Unit)
                .ThenInclude(unit => unit!.EmergencyService)
            .Include(u => u.Hospital)
            .FirstOrDefaultAsync(u => u.Id == id && u.Active);
    }

    public IQueryable<User> Query()
    {
        return _context.Users
            .AsNoTracking()
            .Include(u => u.AccessPermissions)
            .Include(u => u.Unit)
                .ThenInclude(unit => unit!.EmergencyService)
            .Include(u => u.Hospital)
            .Where(u => u.Active)
            .OrderBy(u => u.FullName);
    }

    public async Task<IEnumerable<User>> GetForSelectAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Active)
            .OrderBy(u => u.FullName)
            .Select(u => new User
            {
                Id = u.Id,
                FullName = u.FullName
            })
            .ToListAsync();
    }

    public IQueryable<User> SearchQuery(string key)
    {
        return Query().Where(u =>
            EF.Functions.ILike(u.Username, $"%{key}%") ||
            EF.Functions.ILike(u.Email, $"%{key}%") ||
            EF.Functions.ILike(u.FullName, $"%{key}%")
        );
    }
    public Task<bool> ExistsByUsernameAsync(string username)
        => _context.Users.AnyAsync(u => u.Username == username);

    public Task<bool> ExistsByEmailAsync(string email)
        => _context.Users.AnyAsync(u => u.Email == email);
}
