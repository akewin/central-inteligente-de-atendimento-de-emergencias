namespace Api.Dtos;

public sealed class OperationalDispatchReadDto
{
    public int Id { get; set; }
    public int OccurrenceId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? ServiceStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitService { get; set; } = string.Empty;
    public string OccurrenceStatus { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public IReadOnlyList<string> Services { get; set; } = [];
    public PatientTransportDto? Transport { get; set; }
}

public sealed class OperationalDispatchTransitionRequestDto
{
    public string TargetStatus { get; set; } = string.Empty;
    public DateTime? ExpectedUpdatedAt { get; set; }
}

public sealed class DispatchRealtimeEventDto
{
    public int DispatchId { get; set; }
    public int OccurrenceId { get; set; }
    public int UnitId { get; set; }
    public string DispatchStatus { get; set; } = string.Empty;
    public string OccurrenceStatus { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}

public sealed class OperationalRouteReadDto
{
    public int DispatchId { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string OriginLabel { get; set; } = string.Empty;
    public string DestinationLabel { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public double EstimatedMinutes { get; set; }
    public IReadOnlyList<MapCoordinateDto> Geometry { get; set; } = [];
    public MapPointDto Origin { get; set; } = new();
    public MapPointDto Destination { get; set; } = new();
}
