using Api.Models;

namespace Api.Interfaces.Repositories;

public interface IDispatchRepository
{
    Task<Occurrence?> GetOccurrenceForConfirmationAsync(int occurrenceId, CancellationToken cancellationToken = default);
    Task<List<Unit>> GetUnitsForConfirmationAsync(IReadOnlyCollection<int> unitIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsForOccurrenceAsync(int occurrenceId, CancellationToken cancellationToken = default);
    void AddRange(IEnumerable<Dispatch> dispatches);
}
