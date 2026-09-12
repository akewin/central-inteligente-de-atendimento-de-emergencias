namespace Api.Dtos;

public sealed class DispatchConfirmationRequestDto
{
    public IReadOnlyList<int>? UnitIds { get; set; }
}

public sealed class DispatchConfirmationReadDto
{
    public int OccurrenceId { get; set; }
    public string OccurrenceStatus { get; set; } = string.Empty;
    public DateTime ConfirmedAt { get; set; }
    public OccurrenceCreatorDto ConfirmedBy { get; set; } = new();
    public IReadOnlyList<DispatchReadDto> Dispatches { get; set; } = Array.Empty<DispatchReadDto>();
}

public sealed class DispatchReadDto
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string UnitStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? ServiceStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
