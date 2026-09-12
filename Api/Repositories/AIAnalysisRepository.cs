using Api.Data;
using Api.Interfaces.Repositories;
using Api.Models;

namespace Api.Repositories;

public sealed class AIAnalysisRepository : IAIAnalysisRepository
{
    private readonly ApiDbContext _context;

    public AIAnalysisRepository(ApiDbContext context)
    {
        _context = context;
    }

    public Task<AIAnalysis> CreateAsync(AIAnalysis analysis)
    {
        _context.AIAnalyses.Add(analysis);
        return Task.FromResult(analysis);
    }
}
