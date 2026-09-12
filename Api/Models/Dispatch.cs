using Api.Models.Common;
using Api.Models.Enums;

namespace Api.Models;

public sealed class Dispatch : AuditableEntity
{
    public int Id { get; set; }
    public int OccurrenceId { get; set; }
    public Occurrence Occurrence { get; set; } = null!;
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
    public int ConfirmedByUserId { get; set; }
    public User ConfirmedByUser { get; set; } = null!;
    public DispatchStatus Status { get; set; } = DispatchStatus.ATRIBUIDO;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ArrivedAt { get; set; }
    public DateTime? ServiceStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
