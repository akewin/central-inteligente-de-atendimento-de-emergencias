using Api.Dtos;
using Api.Helpers.Pagination;
using Api.Models;

namespace Api.Interfaces.Repositories;

public interface ISystemLogRepository
{
    Task CreateAsync(SystemLog log);

    Task<SystemLogReadDto?> GetByIdAsync(int id);

    Task<PagedResult<SystemLogListDto>> GetPagedAsync(
        int? userId,
        string? action,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize
    );

    IQueryable<SystemLog> Query();
}
