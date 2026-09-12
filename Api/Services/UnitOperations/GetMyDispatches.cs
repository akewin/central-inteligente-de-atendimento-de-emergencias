using Api.Data;
using Api.Dtos;
using Api.Helpers.Pagination;
using Api.Middlewares;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UnitOperations;

public sealed class GetMyDispatches
{
    private readonly ApiDbContext _context;
    private readonly OperationalUserResolver _operationalUser;
    public GetMyDispatches(ApiDbContext context, OperationalUserResolver operationalUser)
        => (_context, _operationalUser) = (context, operationalUser);

    public async Task<PagedResult<OperationalDispatchReadDto>> ExecuteAsync(
        string scope, int page, int pageSize, CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 100) throw new AppException("Paginação inválida.");
        if (scope is not "active" and not "history") throw new AppException("Use scope active ou history.");
        var user = await _operationalUser.ResolveAsync(cancellationToken);
        var terminal = new[] { DispatchStatus.CONCLUIDO, DispatchStatus.CANCELADO };
        var query = _context.Dispatches.AsNoTracking()
            .Include(item => item.Unit).ThenInclude(unit => unit.EmergencyService)
            .Include(item => item.Occurrence)
                .ThenInclude(occurrence => occurrence.PatientTransports).ThenInclude(transport => transport.Hospital)
            .Include(item => item.Occurrence)
                .ThenInclude(occurrence => occurrence.PatientTransports).ThenInclude(transport => transport.HospitalWard)
            .Where(item => item.UnitId == user.UnitId &&
                (scope == "history" ? terminal.Contains(item.Status) : !terminal.Contains(item.Status)));
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(item => item.CreatedAt).ThenByDescending(item => item.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResult<OperationalDispatchReadDto>(total, page, pageSize, items.Select(OperationalDispatchMapper.ToDto));
    }
}
