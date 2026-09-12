namespace Api.Dtos;

public sealed class OccurrenceUnitRecommendationsDto
{
    public int OccurrenceId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public IReadOnlyList<UnitRecommendationGroupDto> Groups { get; set; } = Array.Empty<UnitRecommendationGroupDto>();
}

public sealed class UnitRecommendationGroupDto
{
    public string Service { get; set; } = string.Empty;
    public string EmergencyNumber { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool HasAvailableUnits => Units.Count > 0;
    public IReadOnlyList<UnitRecommendationDto> Units { get; set; } = Array.Empty<UnitRecommendationDto>();
}

public sealed class UnitRecommendationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
}
