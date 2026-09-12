using Api.Dtos;
using Api.Models;

namespace Api.Mappers;

public static class AIAnalysisMapper
{
    public static AIAnalysisReadDto ToReadDto(AIAnalysis analysis)
    {
        var services = new List<string>(3);
        if (analysis.RecommendsPolice) services.Add("POLICIA");
        if (analysis.RecommendsSamu) services.Add("SAMU");
        if (analysis.RecommendsFireDepartment) services.Add("BOMBEIROS");

        return new AIAnalysisReadDto
        {
            Id = analysis.Id,
            OccurrenceId = analysis.OccurrenceId,
            RecommendedType = analysis.RecommendedType.ToString(),
            RecommendedPriority = analysis.RecommendedPriority.ToString(),
            RecommendedServices = services,
            Reason = analysis.Reason,
            Provider = analysis.Provider,
            Model = analysis.Model,
            CreatedAt = analysis.CreatedAt
        };
    }
}
