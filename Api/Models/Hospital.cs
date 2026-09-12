using Api.Models.Common;

namespace Api.Models;

public sealed class Hospital : AuditableEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = "Araras";
    public string State { get; set; } = "SP";
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public bool HasEmergencyDepartment { get; set; } = true;
    public bool Active { get; set; } = true;
    public ICollection<HospitalWard> Wards { get; set; } = new List<HospitalWard>();
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<PatientTransport> Transports { get; set; } = new List<PatientTransport>();
}
