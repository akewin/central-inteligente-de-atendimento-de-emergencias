using Api.Models.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class AccessPermission : AuditableEntity
{
  public int Id { get; set; }

  public int UserId { get; set; }
  public int SystemResourceId { get; set; }

  public User User { get; set; } = null!;
  public SystemResource SystemResource { get; set; } = null!;
}
