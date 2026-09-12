using System.Text.Json.Serialization;

namespace Api.Dtos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class AIAnalysisLlmOutputDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("priority")]
    public string? Priority { get; set; }

    [JsonPropertyName("recommendedServices")]
    public string[]? RecommendedServices { get; set; }

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }
}
