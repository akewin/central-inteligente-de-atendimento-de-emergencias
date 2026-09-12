using Api.Models.Common;
using Api.Models.Enums;

namespace Api.Models;

public sealed class Unit : AuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string NormalizedName { get; set; } = null!;
    public int EmergencyServiceId { get; set; }
    public EmergencyService EmergencyService { get; set; } = null!;
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
    public UnitStatus Status { get; set; } = UnitStatus.DISPONIVEL;
    public ICollection<Dispatch> Dispatches { get; set; } = new List<Dispatch>();
    public User? OperationalUser { get; set; }
}
