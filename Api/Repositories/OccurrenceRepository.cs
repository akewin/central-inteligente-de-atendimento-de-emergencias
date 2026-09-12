using Api.Data;
using Api.Interfaces.Repositories;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class OccurrenceRepository : IOccurrenceRepository
{
    private readonly ApiDbContext _context;

    public OccurrenceRepository(ApiDbContext context)
    {
        _context = context;
    }

    public Task<Occurrence> CreateAsync(Occurrence occurrence)
    {
        _context.Occurrences.Add(occurrence);
        return Task.FromResult(occurrence);
    }

    public Task<Occurrence?> GetByIdAsync(int id)
    {
        return _context.Occurrences
            .AsNoTracking()
            .Include(o => o.CreatedByUser)
            .Include(o => o.AIAnalyses)
            .Include(o => o.ServicesConfirmedByUser)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public Task<Occurrence?> GetForAnalysisAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Occurrences
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public Task<Occurrence?> GetForServiceConfirmationAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Occurrences
            .Include(o => o.AIAnalyses)
            .Include(o => o.ServicesConfirmedByUser)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public Task<Occurrence?> GetForUnitRecommendationAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return _context.Occurrences
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public Task<bool> CreatorExistsAsync(int userId)
    {
        return _context.Users.AnyAsync(u => u.Id == userId && u.Active);
    }

    public IQueryable<Occurrence> Query()
    {
        return _context.Occurrences
            .AsNoTracking()
            .Include(o => o.CreatedByUser);
    }
}
