using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;

namespace Api.Validations;

public sealed class OccurrenceStatusTransitionValidator
{
    private static readonly HashSet<OccurrenceStatus> HumanTargets =
        [OccurrenceStatus.EM_ATENDIMENTO, OccurrenceStatus.FINALIZADA, OccurrenceStatus.CANCELADA];

    public ValidatedOccurrenceStatusTransition Validate(OccurrenceStatusTransitionRequestDto? dto)
    {
        if (dto is null) throw new AppException("Informe a transição de status.");
        if (!Enum.TryParse<OccurrenceStatus>(dto.TargetStatus?.Trim(), true, out var target) || !HumanTargets.Contains(target))
            throw new AppException("Status de destino inválido para uma ação do atendente.");
        if (!dto.ExpectedUpdatedAt.HasValue)
            throw new AppException("A versão atual da ocorrência é obrigatória. Atualize a página e tente novamente.");

        var expected = dto.ExpectedUpdatedAt.Value;
        if (expected.Kind == DateTimeKind.Unspecified) expected = DateTime.SpecifyKind(expected, DateTimeKind.Utc);
        expected = expected.ToUniversalTime();

        var reason = dto.Reason?.Trim();
        if (reason?.Length > 500) throw new AppException("A justificativa deve ter no máximo 500 caracteres.");
        if (target == OccurrenceStatus.CANCELADA && (reason is null || reason.Length < 5))
            throw new AppException("Informe uma justificativa de cancelamento com pelo menos 5 caracteres.");

        return new ValidatedOccurrenceStatusTransition(target, expected, reason);
    }
}

public sealed record ValidatedOccurrenceStatusTransition(
    OccurrenceStatus TargetStatus,
    DateTime ExpectedUpdatedAt,
    string? Reason);
