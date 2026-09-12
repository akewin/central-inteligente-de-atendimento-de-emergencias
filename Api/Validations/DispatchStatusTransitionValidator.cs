using System.Net;
using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;

namespace Api.Validations;

public sealed class DispatchStatusTransitionValidator
{
    private static readonly IReadOnlyDictionary<DispatchStatus, DispatchStatus> Next =
        new Dictionary<DispatchStatus, DispatchStatus>
        {
            [DispatchStatus.ATRIBUIDO] = DispatchStatus.ACEITO,
            [DispatchStatus.ACEITO] = DispatchStatus.NO_LOCAL,
            [DispatchStatus.NO_LOCAL] = DispatchStatus.EM_ATENDIMENTO,
            [DispatchStatus.EM_ATENDIMENTO] = DispatchStatus.CONCLUIDO
        };

    public (DispatchStatus Target, DateTime ExpectedUpdatedAt) Validate(
        DispatchStatus current,
        OperationalDispatchTransitionRequestDto? dto)
    {
        if (dto is null) throw new AppException("Os dados da transição são obrigatórios.");
        if (!Enum.TryParse<DispatchStatus>(dto.TargetStatus?.Trim(), false, out var target) || !Enum.IsDefined(target))
            throw new AppException("Estado operacional inválido.");
        if (target == DispatchStatus.CANCELADO)
            throw new AppException("Somente a central pode cancelar um despacho.", (int)HttpStatusCode.Forbidden);
        if (!Next.TryGetValue(current, out var expected) || expected != target)
            throw new AppException($"A transição de {current} para {target} não é permitida.", (int)HttpStatusCode.Conflict);
        if (!dto.ExpectedUpdatedAt.HasValue)
            throw new AppException("A versão atual do despacho é obrigatória.");
        var version = dto.ExpectedUpdatedAt.Value;
        return (target, version.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(version, DateTimeKind.Utc) : version.ToUniversalTime());
    }
}
