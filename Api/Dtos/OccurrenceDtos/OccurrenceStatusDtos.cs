namespace Api.Dtos;

public sealed class OccurrenceStatusTransitionRequestDto
{
    public string TargetStatus { get; set; } = string.Empty;
    public DateTime? ExpectedUpdatedAt { get; set; }
    public string? Reason { get; set; }
}

public sealed class OccurrenceStatusTransitionReadDto
{
    public int OccurrenceId { get; set; }
    public string PreviousStatus { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public OccurrenceCreatorDto ChangedBy { get; set; } = new();
    public IReadOnlyList<OccurrenceUnitStatusChangeDto> Units { get; set; } = [];
}

public sealed class OccurrenceUnitStatusChangeDto
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
}

public sealed class OccurrenceTimelineItemDto
{
    public int Id { get; set; }
    public string Event { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string? CurrentStatus { get; set; }
    public string? Reason { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
