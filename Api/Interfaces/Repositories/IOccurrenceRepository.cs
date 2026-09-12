using Api.Models;

namespace Api.Interfaces.Repositories;

public interface IOccurrenceRepository
{
    Task<Occurrence> CreateAsync(Occurrence occurrence);
    Task<Occurrence?> GetByIdAsync(int id);
    Task<Occurrence?> GetForAnalysisAsync(int id, CancellationToken cancellationToken = default);
    Task<Occurrence?> GetForServiceConfirmationAsync(int id, CancellationToken cancellationToken = default);
    Task<Occurrence?> GetForUnitRecommendationAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> CreatorExistsAsync(int userId);
    IQueryable<Occurrence> Query();
}
