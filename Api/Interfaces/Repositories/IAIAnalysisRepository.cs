using Api.Models;

namespace Api.Interfaces.Repositories;

public interface IAIAnalysisRepository
{
    Task<AIAnalysis> CreateAsync(AIAnalysis analysis);
}
