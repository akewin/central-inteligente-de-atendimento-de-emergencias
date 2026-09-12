namespace Api.Dtos;

public sealed class AIAnalysisReadDto
{
    public int Id { get; set; }
    public int OccurrenceId { get; set; }
    public string RecommendedType { get; set; } = string.Empty;
    public string RecommendedPriority { get; set; } = string.Empty;
    public IReadOnlyList<string> RecommendedServices { get; set; } = Array.Empty<string>();
    public string Reason { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
