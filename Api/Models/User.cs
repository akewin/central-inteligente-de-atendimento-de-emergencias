using Api.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class User : AuditableEntity
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;

    public bool Active { get; set; } = true;
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }
    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }

    public ICollection<AccessPermission> AccessPermissions { get; set; } = new List<AccessPermission>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Occurrence> Occurrences { get; set; } = new List<Occurrence>();
    public ICollection<Occurrence> ConfirmedServiceOccurrences { get; set; } = new List<Occurrence>();
    public ICollection<Dispatch> ConfirmedDispatches { get; set; } = new List<Dispatch>();
}
