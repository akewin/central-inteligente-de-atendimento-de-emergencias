using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Api.Services.UnitOperations;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Dispatches;

public sealed class GetOccurrenceDispatches
{
    private readonly ApiDbContext _context;
    public GetOccurrenceDispatches(ApiDbContext context) => _context = context;

    public async Task<IReadOnlyList<OperationalDispatchReadDto>> ExecuteAsync(int occurrenceId, CancellationToken cancellationToken)
    {
        if (!await _context.Occurrences.AsNoTracking().AnyAsync(item => item.Id == occurrenceId, cancellationToken))
            throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        var dispatches = await _context.Dispatches.AsNoTracking()
            .Include(item => item.Unit).ThenInclude(unit => unit.EmergencyService)
            .Include(item => item.Occurrence)
            .Where(item => item.OccurrenceId == occurrenceId)
            .OrderBy(item => item.Unit.EmergencyService.Type).ThenBy(item => item.Unit.Name)
            .ToListAsync(cancellationToken);
        return dispatches.Select(OperationalDispatchMapper.ToDto).ToList();
    }
}
