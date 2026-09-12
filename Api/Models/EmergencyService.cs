using Api.Models.Common;
using Api.Models.Enums;

namespace Api.Models;

public sealed class EmergencyService : AuditableEntity
{
    public int Id { get; set; }
    public EmergencyServiceType Type { get; set; }
    public string EmergencyNumber { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
