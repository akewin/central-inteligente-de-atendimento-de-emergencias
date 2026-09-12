using Api.Models.Common;

namespace Api.Models;

public sealed class HospitalWard : AuditableEntity
{
    public int Id { get; set; }
    public int HospitalId { get; set; }
    public Hospital Hospital { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int TotalBeds { get; set; }
    public int OccupiedBeds { get; set; }
    public int ReservedBeds { get; set; }
    public bool Active { get; set; } = true;
    public int AvailableBeds => Math.Max(0, TotalBeds - OccupiedBeds - ReservedBeds);
    public ICollection<PatientTransport> Transports { get; set; } = new List<PatientTransport>();
}
