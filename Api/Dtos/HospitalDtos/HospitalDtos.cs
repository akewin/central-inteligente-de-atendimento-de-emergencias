namespace Api.Dtos;

public sealed class HospitalWardDto
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int ReservedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public bool Active { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class HospitalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool HasEmergencyDepartment { get; set; }
    public bool Active { get; set; }
    public IReadOnlyList<HospitalWardDto> Wards { get; set; } = [];
    public DateTime UpdatedAt { get; set; }
}

public sealed class HospitalSaveDto
{
    public string Name { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool HasEmergencyDepartment { get; set; } = true;
    public bool Active { get; set; } = true;
    public DateTime? ExpectedUpdatedAt { get; set; }
}

public sealed class HospitalWardSaveDto
{
    public string Name { get; set; } = string.Empty;
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public bool Active { get; set; } = true;
    public DateTime? ExpectedUpdatedAt { get; set; }
}

public sealed class PatientTransportDto
{
    public int Id { get; set; }
    public int OccurrenceId { get; set; }
    public int RequestedByDispatchId { get; set; }
    public int? SamuDispatchId { get; set; }
    public int? SamuUnitId { get; set; }
    public int? HospitalId { get; set; }
    public int? HospitalWardId { get; set; }
    public string? HospitalName { get; set; }
    public string? WardName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? OperationalNotes { get; set; }
    public string? Priority { get; set; }
    public string? Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public double? EstimatedMinutes { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class RequestTransportDto { public string? OperationalNotes { get; set; } }
public sealed class AssignSamuDto { public int UnitId { get; set; } }
public sealed class AssignTransportDestinationDto
{
    public int HospitalId { get; set; }
    public int HospitalWardId { get; set; }
    public string? OperationalNotes { get; set; }
    public DateTime? ExpectedUpdatedAt { get; set; }
}

public sealed class HospitalReceptionActionDto
{
    public string Action { get; set; } = string.Empty;
    public DateTime? ExpectedUpdatedAt { get; set; }
}
