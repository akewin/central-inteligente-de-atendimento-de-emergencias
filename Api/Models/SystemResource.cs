using Api.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class SystemResource : AuditableEntity
{
  public int Id { get; set; }

  public string Name { get; set; } = null!;
  public string ExhibitionName { get; set; } = null!;

  public bool Active { get; set; } = true;

  public ICollection<AccessPermission> AccessPermissions { get; set; } = new List<AccessPermission>();
}
