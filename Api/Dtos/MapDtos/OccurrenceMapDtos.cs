namespace Api.Dtos;

public sealed class OccurrenceMapDto
{
    public int OccurrenceId { get; set; }
    public DateTime GeneratedAt { get; set; }
    public MapPointDto Victim { get; set; } = new();
    public IReadOnlyList<MapUnitDto> Ambulances { get; set; } = Array.Empty<MapUnitDto>();
    public IReadOnlyList<MapHospitalDto> Hospitals { get; set; } = Array.Empty<MapHospitalDto>();
    public bool HospitalSearchSucceeded { get; set; } = true;
    public int? SelectedHospitalId { get; set; }
    public bool HospitalSelectionConfirmed { get; set; }
    public IReadOnlyList<MapRouteDto> Routes { get; set; } = Array.Empty<MapRouteDto>();
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
}

public sealed class HospitalSelectionRequestDto
{
    public int HospitalId { get; set; }
}

public sealed class HospitalSelectionReadDto
{
    public int OccurrenceId { get; set; }
    public int HospitalId { get; set; }
    public string HospitalName { get; set; } = string.Empty;
    public string HospitalAddress { get; set; } = string.Empty;
    public DateTime SelectedAt { get; set; }
}

public sealed class MapPointDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class MapUnitDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}

public sealed class MapHospitalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool HasEmergencyDepartment { get; set; }
    public double StraightLineDistanceKm { get; set; }
    public double? RoadDistanceKm { get; set; }
    public double? EstimatedMinutes { get; set; }
}

public sealed class MapRouteDto
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int HospitalId { get; set; }
    public double TotalDistanceKm { get; set; }
    public double TotalEstimatedMinutes { get; set; }
    public double UnitToVictimDistanceKm { get; set; }
    public double UnitToVictimMinutes { get; set; }
    public double VictimToHospitalDistanceKm { get; set; }
    public double VictimToHospitalMinutes { get; set; }
    public IReadOnlyList<MapCoordinateDto> Geometry { get; set; } = Array.Empty<MapCoordinateDto>();
}

public sealed class MapCoordinateDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
}
