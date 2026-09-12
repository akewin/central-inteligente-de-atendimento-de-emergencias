using Api.Middlewares;
using Api.Models.Enums;

namespace Api.Services.Occurrences;

public sealed class OccurrenceStateMachine
{
    private static readonly IReadOnlyDictionary<OccurrenceStatus, IReadOnlySet<OccurrenceStatus>> Allowed =
        new Dictionary<OccurrenceStatus, IReadOnlySet<OccurrenceStatus>>
        {
            [OccurrenceStatus.ABERTA] = new HashSet<OccurrenceStatus> { OccurrenceStatus.EM_ANALISE, OccurrenceStatus.CANCELADA },
            [OccurrenceStatus.EM_ANALISE] = new HashSet<OccurrenceStatus> { OccurrenceStatus.ABERTA, OccurrenceStatus.AGUARDANDO_CONFIRMACAO },
            [OccurrenceStatus.AGUARDANDO_CONFIRMACAO] = new HashSet<OccurrenceStatus> { OccurrenceStatus.DESPACHADA, OccurrenceStatus.CANCELADA },
            [OccurrenceStatus.DESPACHADA] = new HashSet<OccurrenceStatus> { OccurrenceStatus.EM_ATENDIMENTO, OccurrenceStatus.CANCELADA },
            [OccurrenceStatus.EM_ATENDIMENTO] = new HashSet<OccurrenceStatus> { OccurrenceStatus.FINALIZADA, OccurrenceStatus.CANCELADA },
            [OccurrenceStatus.FINALIZADA] = new HashSet<OccurrenceStatus>(),
            [OccurrenceStatus.CANCELADA] = new HashSet<OccurrenceStatus>()
        };

    public void EnsureAllowed(OccurrenceStatus current, OccurrenceStatus target)
    {
        if (!Allowed.TryGetValue(current, out var targets) || !targets.Contains(target))
            throw new AppException($"A transição de {current} para {target} não é permitida.", StatusCodes.Status409Conflict);
    }
}
