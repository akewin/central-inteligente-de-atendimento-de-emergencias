using Api.Helpers.Pagination;
using Api.Models;

namespace Api.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> SoftDeleteAsync(int id);

    Task<User?> GetByIdAsync(int id);
    IQueryable<User> Query();
    Task<IEnumerable<User>> GetForSelectAsync();
    IQueryable<User> SearchQuery(string key);

    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
}
