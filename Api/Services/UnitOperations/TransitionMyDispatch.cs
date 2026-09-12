using System.Data;
using System.Net;
using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Realtime;
using Api.Validations;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UnitOperations;

public sealed class TransitionMyDispatch
{
    private readonly ApiDbContext _context;
    private readonly OperationalUserResolver _operationalUser;
    private readonly DispatchStatusTransitionValidator _validator;
    private readonly CreateSystemLog _audit;
    private readonly IDispatchRealtimeNotifier _realtime;
    private readonly ILogger<TransitionMyDispatch> _logger;

    public TransitionMyDispatch(ApiDbContext context, OperationalUserResolver operationalUser,
        DispatchStatusTransitionValidator validator, CreateSystemLog audit,
        IDispatchRealtimeNotifier realtime, ILogger<TransitionMyDispatch> logger)
        => (_context, _operationalUser, _validator, _audit, _realtime, _logger)
            = (context, operationalUser, validator, audit, realtime, logger);

    public async Task<OperationalDispatchReadDto> ExecuteAsync(
        int dispatchId, OperationalDispatchTransitionRequestDto? dto, CancellationToken cancellationToken)
    {
        if (dispatchId <= 0) throw new AppException("Identificador inválido.");
        var user = await _operationalUser.ResolveAsync(cancellationToken);
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var dispatch = await _context.Dispatches
            .Include(item => item.Unit).ThenInclude(unit => unit.EmergencyService)
            .Include(item => item.Occurrence).ThenInclude(occurrence => occurrence.Dispatches)
            .SingleOrDefaultAsync(item => item.Id == dispatchId && item.UnitId == user.UnitId, cancellationToken)
            ?? throw new AppException("Chamado não encontrado para a equipe autenticada.", (int)HttpStatusCode.NotFound);

        if (dispatch.Occurrence.Status is OccurrenceStatus.CANCELADA or OccurrenceStatus.FINALIZADA)
            throw new AppException("A ocorrência não aceita novas ações operacionais.", (int)HttpStatusCode.Conflict);
        var input = _validator.Validate(dispatch.Status, dto);
        _context.Entry(dispatch).Property(item => item.UpdatedAt).OriginalValue = input.ExpectedUpdatedAt;
        var previous = dispatch.Status;
        ApplyTransition(dispatch, input.Target);
        ApplyOccurrenceEffects(dispatch);
        if (dispatch.Occurrence.Status == OccurrenceStatus.FINALIZADA && await _context.PatientTransports.AnyAsync(
                transport => transport.OccurrenceId == dispatch.OccurrenceId
                    && transport.Status != TransportStatus.RECEBIDO && transport.Status != TransportStatus.CANCELADO,
                cancellationToken))
            dispatch.Occurrence.Status = OccurrenceStatus.EM_ATENDIMENTO;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _audit.ExecuteAsync(SystemLogActionFactory.Update("DispatchStatus", dispatch.Id), data: new SystemLogDataDto
            {
                Type = "update",
                PrevState = new { DispatchStatus = previous.ToString(), UnitStatus = PreviousUnitStatus(previous) },
                CurrState = new
                {
                    DispatchStatus = dispatch.Status.ToString(),
                    UnitStatus = dispatch.Unit.Status.ToString(),
                    dispatch.UnitId,
                    dispatch.OccurrenceId,
                    OccurrenceStatus = dispatch.Occurrence.Status.ToString()
                }
            });
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException("O chamado foi atualizado por outro usuário. Recarregue e tente novamente.", (int)HttpStatusCode.Conflict);
        }
        catch (DbUpdateException)
        {
            throw new AppException("A transição conflitou com outra operação em andamento.", (int)HttpStatusCode.Conflict);
        }

        var message = new DispatchRealtimeEventDto
        {
            DispatchId = dispatch.Id, OccurrenceId = dispatch.OccurrenceId, UnitId = dispatch.UnitId,
            DispatchStatus = dispatch.Status.ToString(), OccurrenceStatus = dispatch.Occurrence.Status.ToString(),
            UpdatedAt = dispatch.UpdatedAt
        };
        try { await _realtime.DispatchStatusChangedAsync(message, cancellationToken); }
        catch (Exception error) { _logger.LogWarning(error, "Falha ao publicar atualização do despacho {DispatchId}", dispatch.Id); }
        return OperationalDispatchMapper.ToDto(dispatch);
    }

    private static void ApplyTransition(Api.Models.Dispatch dispatch, DispatchStatus target)
    {
        var now = DateTime.UtcNow;
        dispatch.Status = target;
        switch (target)
        {
            case DispatchStatus.ACEITO:
                dispatch.AcceptedAt = now; dispatch.Unit.Status = UnitStatus.DESLOCAMENTO; break;
            case DispatchStatus.NO_LOCAL:
                dispatch.ArrivedAt = now; dispatch.Unit.Status = UnitStatus.EM_ATENDIMENTO; break;
            case DispatchStatus.EM_ATENDIMENTO:
                dispatch.ServiceStartedAt = now; dispatch.Unit.Status = UnitStatus.EM_ATENDIMENTO; break;
            case DispatchStatus.CONCLUIDO:
                dispatch.CompletedAt = now; dispatch.Unit.Status = UnitStatus.DISPONIVEL; break;
        }
    }

    private static void ApplyOccurrenceEffects(Api.Models.Dispatch dispatch)
    {
        if (dispatch.Status == DispatchStatus.EM_ATENDIMENTO && dispatch.Occurrence.Status == OccurrenceStatus.DESPACHADA)
            dispatch.Occurrence.Status = OccurrenceStatus.EM_ATENDIMENTO;
        if (dispatch.Status != DispatchStatus.CONCLUIDO) return;
        var statuses = dispatch.Occurrence.Dispatches.Select(item => item.Id == dispatch.Id ? dispatch.Status : item.Status).ToList();
        if (statuses.Any(status => status == DispatchStatus.CONCLUIDO) &&
            statuses.All(status => status is DispatchStatus.CONCLUIDO or DispatchStatus.CANCELADO))
            dispatch.Occurrence.Status = OccurrenceStatus.FINALIZADA;
    }

    private static string PreviousUnitStatus(DispatchStatus status) => status switch
    {
        DispatchStatus.ATRIBUIDO => UnitStatus.RESERVADA.ToString(),
        DispatchStatus.ACEITO => UnitStatus.DESLOCAMENTO.ToString(),
        DispatchStatus.NO_LOCAL or DispatchStatus.EM_ATENDIMENTO => UnitStatus.EM_ATENDIMENTO.ToString(),
        _ => UnitStatus.DISPONIVEL.ToString()
    };
}
