namespace Api.Dtos;

public sealed class OccurrenceServiceConfirmationReadDto
{
    public IReadOnlyList<string> ConfirmedServices { get; set; } = Array.Empty<string>();
    public OccurrenceCreatorDto ConfirmedBy { get; set; } = new();
    public DateTime ConfirmedAt { get; set; }
}
