namespace Api.Dtos;

public class OccurrenceReadDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string LocationDescription { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Reference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ConfirmedType { get; set; }
    public string? ConfirmedPriority { get; set; }
    public OccurrenceCreatorDto CreatedBy { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public AIAnalysisReadDto? LatestAIAnalysis { get; set; }
    public OccurrenceServiceConfirmationReadDto? ServiceConfirmation { get; set; }
}
