using Api.Dtos;

namespace Api.Services.Maps;

public static class HospitalRecommendationRanker
{
    public static IReadOnlyList<MapHospitalDto> Rank(
        IEnumerable<MapHospitalDto> hospitals,
        int limit)
        => hospitals
            .OrderByDescending(item => item.HasEmergencyDepartment)
            .ThenBy(item => item.EstimatedMinutes ?? double.MaxValue)
            .ThenBy(item => item.RoadDistanceKm ?? item.StraightLineDistanceKm)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();
}
