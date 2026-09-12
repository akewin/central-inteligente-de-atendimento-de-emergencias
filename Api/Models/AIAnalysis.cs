using Api.Models.Common;
using Api.Models.Enums;

namespace Api.Models;

public class AIAnalysis : AuditableEntity
{
    public int Id { get; set; }
    public int OccurrenceId { get; set; }
    public Occurrence Occurrence { get; set; } = null!;
    public OccurrenceType RecommendedType { get; set; }
    public OccurrencePriority RecommendedPriority { get; set; }
    public bool RecommendsPolice { get; set; }
    public bool RecommendsSamu { get; set; }
    public bool RecommendsFireDepartment { get; set; }
    public string Reason { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string Model { get; set; } = null!;
}
