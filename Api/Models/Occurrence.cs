using Api.Models.Common;
using Api.Models.Enums;

namespace Api.Models;

public class Occurrence : AuditableEntity
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;
    public string LocationDescription { get; set; } = null!;
    public string? PostalCode { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? AddressReference { get; set; }
    public string? GeocodingSource { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int? SelectedHospitalId { get; set; }
    public Hospital? SelectedHospital { get; set; }
    public string? SelectedHospitalName { get; set; }
    public string? SelectedHospitalAddress { get; set; }
    public decimal? SelectedHospitalLatitude { get; set; }
    public decimal? SelectedHospitalLongitude { get; set; }
    public DateTime? HospitalSelectedAt { get; set; }

    public OccurrenceType? ConfirmedType { get; set; }
    public OccurrencePriority? ConfirmedPriority { get; set; }
    public OccurrenceStatus Status { get; set; } = OccurrenceStatus.ABERTA;

    public bool PoliceConfirmed { get; set; }
    public bool SamuConfirmed { get; set; }
    public bool FireDepartmentConfirmed { get; set; }
    public int? ServicesConfirmedByUserId { get; set; }
    public User? ServicesConfirmedByUser { get; set; }
    public DateTime? ServicesConfirmedAt { get; set; }

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public ICollection<AIAnalysis> AIAnalyses { get; set; } = new List<AIAnalysis>();
    public ICollection<Dispatch> Dispatches { get; set; } = new List<Dispatch>();
    public ICollection<PatientTransport> PatientTransports { get; set; } = new List<PatientTransport>();
}
