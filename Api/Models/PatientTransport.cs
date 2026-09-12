using Api.Models.Common;
using Api.Models.Enums;

namespace Api.Models;

public sealed class PatientTransport : AuditableEntity
{
    public int Id { get; set; }
    public int OccurrenceId { get; set; }
    public Occurrence Occurrence { get; set; } = null!;
    public int RequestedByDispatchId { get; set; }
    public Dispatch RequestedByDispatch { get; set; } = null!;
    public int? SamuDispatchId { get; set; }
    public Dispatch? SamuDispatch { get; set; }
    public int? HospitalId { get; set; }
    public Hospital? Hospital { get; set; }
    public int? HospitalWardId { get; set; }
    public HospitalWard? HospitalWard { get; set; }
    public TransportStatus Status { get; set; }
    public string? OperationalNotes { get; set; }
    public double? EstimatedMinutes { get; set; }
    public DateTime? HospitalNotifiedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? TransportStartedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
