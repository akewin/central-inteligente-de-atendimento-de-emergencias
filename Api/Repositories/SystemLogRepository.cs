using Api.Auditing;
using Api.Data;
using Api.Dtos;
using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class SystemLogRepository : ISystemLogRepository
{
    private readonly ApiDbContext _context;

    public SystemLogRepository(ApiDbContext context)
    {
        _context = context;
    }
    public async Task CreateAsync(SystemLog log)
    {
        _context.SystemLogs.Add(log);
        await _context.SaveChangesAsync();
    }
    public async Task<SystemLogReadDto?> GetByIdAsync(int id)
    {
        return await _context.SystemLogs
            .AsNoTracking()
            .Include(sl => sl.User)
            .Where(sl => sl.Id == id)
            .Select(sl => new SystemLogReadDto
            {
                Id = sl.Id,
                Action = sl.Action,
                GeneratedBy = sl.GeneratedBy ?? string.Empty,
                IpAddress = sl.IpAddress,
                CreatedAt = sl.CreatedAt,
                Data = SystemLogDataSerializer.Deserialize(sl.Data)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResult<SystemLogListDto>> GetPagedAsync(
        int? userId,
        string? action,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize
    )
    {
        var query = _context.SystemLogs
            .AsNoTracking()
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(sl => sl.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(action))
        {
            var actionFilter = action.ToLower();
            query = query.Where(sl =>
                EF.Functions.Like(
                    sl.Action.ToLower(),
                    $"%{actionFilter}%"
                )
            );
        }

        if (startDate.HasValue)
            query = query.Where(sl => sl.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(sl => sl.CreatedAt < endDate.Value);

        query = query.OrderByDescending(sl => sl.CreatedAt);

        var projection = query.Select(sl => new SystemLogListDto
        {
            Id = sl.Id,
            GeneratedBy = sl.GeneratedBy ?? string.Empty,
            Action = sl.Action,
            CreatedAt = sl.CreatedAt
        });

        return await PagedResult<SystemLogListDto>.CreateAsync(
            projection,
            page,
            pageSize
        );
    }

    public IQueryable<SystemLog> Query()
    {
        return _context.SystemLogs
            .AsNoTracking();
    }
}
