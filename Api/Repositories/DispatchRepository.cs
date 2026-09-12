using Api.Data;
using Api.Interfaces.Repositories;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public sealed class DispatchRepository : IDispatchRepository
{
    private readonly ApiDbContext _context;
    public DispatchRepository(ApiDbContext context) => _context = context;

    public Task<Occurrence?> GetOccurrenceForConfirmationAsync(int occurrenceId, CancellationToken cancellationToken = default)
        => _context.Occurrences.FirstOrDefaultAsync(occurrence => occurrence.Id == occurrenceId, cancellationToken);

    public Task<List<Unit>> GetUnitsForConfirmationAsync(IReadOnlyCollection<int> unitIds, CancellationToken cancellationToken = default)
        => _context.Units.Include(unit => unit.EmergencyService)
            .Where(unit => unitIds.Contains(unit.Id)).OrderBy(unit => unit.Id).ToListAsync(cancellationToken);

    public Task<bool> ExistsForOccurrenceAsync(int occurrenceId, CancellationToken cancellationToken = default)
        => _context.Dispatches.AnyAsync(dispatch => dispatch.OccurrenceId == occurrenceId, cancellationToken);

    public void AddRange(IEnumerable<Dispatch> dispatches) => _context.Dispatches.AddRange(dispatches);
}
