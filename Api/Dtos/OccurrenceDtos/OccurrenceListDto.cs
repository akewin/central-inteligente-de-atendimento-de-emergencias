namespace Api.Dtos;

public class OccurrenceListDto
{
    public int Id { get; set; }
    public string DescriptionSummary { get; set; } = string.Empty;
    public string LocationDescription { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ConfirmedType { get; set; }
    public string? ConfirmedPriority { get; set; }
    public OccurrenceCreatorDto CreatedBy { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
