using Api.Data;
using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;
using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public sealed class UnitRepository : IUnitRepository
{
    private readonly ApiDbContext _context;
    public UnitRepository(ApiDbContext context) => _context = context;

    public Task<Unit> CreateAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        _context.Units.Add(unit);
        return Task.FromResult(unit);
    }

    public Task<Unit?> GetByIdAsync(int id, bool tracked = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Unit> query = _context.Units.Include(unit => unit.EmergencyService);
        if (!tracked) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(unit => unit.Id == id, cancellationToken);
    }

    public Task<EmergencyService?> GetServiceAsync(EmergencyServiceType type, CancellationToken cancellationToken = default)
        => _context.EmergencyServices.FirstOrDefaultAsync(service => service.Type == type, cancellationToken);

    public Task<bool> NameExistsAsync(string normalizedName, int? exceptId = null, CancellationToken cancellationToken = default)
        => _context.Units.AnyAsync(unit => unit.NormalizedName == normalizedName && (!exceptId.HasValue || unit.Id != exceptId), cancellationToken);

    public Task<PagedResult<Unit>> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        EmergencyServiceType? service,
        UnitStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Units.AsNoTracking().Include(unit => unit.EmergencyService).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(unit => EF.Functions.ILike(unit.Name, $"%{search.Trim()}%"));
        if (service.HasValue)
            query = query.Where(unit => unit.EmergencyService.Type == service.Value);
        if (status.HasValue)
            query = query.Where(unit => unit.Status == status.Value);

        return PagedResult<Unit>.CreateAsync(query.OrderBy(unit => unit.Name), page, pageSize);
    }

    public Task<List<EmergencyService>> GetServicesAsync(
        IReadOnlyCollection<EmergencyServiceType> services,
        CancellationToken cancellationToken = default)
        => _context.EmergencyServices
            .AsNoTracking()
            .Where(service => services.Contains(service.Type))
            .OrderBy(service => service.Id)
            .ToListAsync(cancellationToken);

    public Task<List<Unit>> GetAvailableByServicesAsync(
        IReadOnlyCollection<EmergencyServiceType> services,
        CancellationToken cancellationToken = default)
        => _context.Units
            .AsNoTracking()
            .Include(unit => unit.EmergencyService)
            .Where(unit => unit.Status == UnitStatus.DISPONIVEL
                && services.Contains(unit.EmergencyService.Type)
                && !unit.Dispatches.Any(dispatch => dispatch.Status != DispatchStatus.CONCLUIDO
                    && dispatch.Status != DispatchStatus.CANCELADO))
            .ToListAsync(cancellationToken);
}
