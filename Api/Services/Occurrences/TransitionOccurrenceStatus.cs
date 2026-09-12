using System.Net;
using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Api.Models;
using Api.Models.Enums;
using Api.Security.Jwt;
using Api.Validations;
using Microsoft.EntityFrameworkCore;
using Api.Realtime;

namespace Api.Services.Occurrences;

public sealed class TransitionOccurrenceStatus
{
    private readonly ApiDbContext _context;
    private readonly OccurrenceStatusTransitionValidator _validator;
    private readonly OccurrenceStateMachine _stateMachine;
    private readonly CurrentUserContext _currentUser;
    private readonly CreateSystemLog _audit;
    private readonly IDispatchRealtimeNotifier _realtime;
    private readonly ILogger<TransitionOccurrenceStatus> _logger;

    public TransitionOccurrenceStatus(
        ApiDbContext context,
        OccurrenceStatusTransitionValidator validator,
        OccurrenceStateMachine stateMachine,
        CurrentUserContext currentUser,
        CreateSystemLog audit, IDispatchRealtimeNotifier realtime, ILogger<TransitionOccurrenceStatus> logger)
        => (_context, _validator, _stateMachine, _currentUser, _audit, _realtime, _logger)
            = (context, validator, stateMachine, currentUser, audit, realtime, logger);

    public async Task<OccurrenceStatusTransitionReadDto> ExecuteAsync(
        int occurrenceId,
        OccurrenceStatusTransitionRequestDto? dto,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositiveInt(occurrenceId);
        var input = _validator.Validate(dto);
        var userId = _currentUser.GetId()
            ?? throw new AppException("Usuário não autenticado.", (int)HttpStatusCode.Unauthorized);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var user = await _context.Users.SingleOrDefaultAsync(item => item.Id == userId && item.Active, cancellationToken)
            ?? throw new AppException("Usuário autenticado não encontrado ou inativo.", (int)HttpStatusCode.Unauthorized);
        var occurrence = await _context.Occurrences
            .Include(item => item.Dispatches)
            .ThenInclude(dispatch => dispatch.Unit)
            .Include(item => item.PatientTransports)
            .ThenInclude(transport => transport.HospitalWard)
            .SingleOrDefaultAsync(item => item.Id == occurrenceId, cancellationToken)
            ?? throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        _context.Entry(occurrence).Property(item => item.UpdatedAt).OriginalValue = input.ExpectedUpdatedAt;
        var previousStatus = occurrence.Status;
        var previousDispatchStatuses = occurrence.Dispatches.ToDictionary(item => item.Id, item => item.Status.ToString());
        _stateMachine.EnsureAllowed(previousStatus, input.TargetStatus);

        if (occurrence.Dispatches.Count > 0 && input.TargetStatus is OccurrenceStatus.EM_ATENDIMENTO or OccurrenceStatus.FINALIZADA)
            throw new AppException("O andamento de uma ocorrência despachada é controlado pelas equipes vinculadas.", (int)HttpStatusCode.Conflict);

        var unitChanges = ApplyUnitEffects(occurrence, input.TargetStatus);
        occurrence.Status = input.TargetStatus;
        if (input.TargetStatus == OccurrenceStatus.CANCELADA)
        {
            foreach (var transport in occurrence.PatientTransports.Where(t => t.Status is not TransportStatus.RECEBIDO and not TransportStatus.CANCELADO))
            {
                if (transport.HospitalWard is not null) transport.HospitalWard.ReservedBeds = Math.Max(0, transport.HospitalWard.ReservedBeds - 1);
                transport.Status = TransportStatus.CANCELADO; transport.CancelledAt = DateTime.UtcNow;
            }
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _audit.ExecuteAsync(
                SystemLogActionFactory.Update("OccurrenceStatus", occurrence.Id),
                data: new SystemLogDataDto
                {
                    Type = "update",
                    PrevState = new { Status = previousStatus.ToString() },
                    CurrState = new
                    {
                        Status = occurrence.Status.ToString(),
                        Reason = input.Reason,
                        Units = unitChanges.Select(change => new
                        {
                            change.UnitId,
                            change.UnitName,
                            change.PreviousStatus,
                            change.CurrentStatus
                        }),
                        Dispatches = occurrence.Dispatches
                            .Where(dispatch => previousDispatchStatuses[dispatch.Id] != dispatch.Status.ToString())
                            .Select(dispatch => new
                            {
                                dispatch.Id,
                                dispatch.UnitId,
                                PreviousStatus = previousDispatchStatuses[dispatch.Id],
                                CurrentStatus = dispatch.Status.ToString(),
                                dispatch.CancelledAt
                            })
                    }
                });
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException(
                "A ocorrência ou uma equipe foi alterada por outro usuário. Atualize os dados e tente novamente.",
                (int)HttpStatusCode.Conflict);
        }

        if (input.TargetStatus == OccurrenceStatus.CANCELADA)
        {
            foreach (var dispatch in occurrence.Dispatches.Where(item => item.Status == DispatchStatus.CANCELADO))
            {
                try
                {
                    await _realtime.DispatchStatusChangedAsync(new DispatchRealtimeEventDto
                    {
                        DispatchId = dispatch.Id, OccurrenceId = occurrence.Id, UnitId = dispatch.UnitId,
                        DispatchStatus = dispatch.Status.ToString(), OccurrenceStatus = occurrence.Status.ToString(),
                        UpdatedAt = dispatch.UpdatedAt
                    }, cancellationToken);
                }
                catch (Exception error) { _logger.LogWarning(error, "Falha ao publicar cancelamento do despacho {DispatchId}", dispatch.Id); }
            }
        }

        return new OccurrenceStatusTransitionReadDto
        {
            OccurrenceId = occurrence.Id,
            PreviousStatus = previousStatus.ToString(),
            CurrentStatus = occurrence.Status.ToString(),
            UpdatedAt = occurrence.UpdatedAt,
            ChangedBy = new OccurrenceCreatorDto { Id = user.Id, Username = user.Username, FullName = user.FullName },
            Units = unitChanges
        };
    }

    private static IReadOnlyList<OccurrenceUnitStatusChangeDto> ApplyUnitEffects(
        Occurrence occurrence,
        OccurrenceStatus target)
    {
        var units = occurrence.Dispatches.Select(dispatch => dispatch.Unit).DistinctBy(unit => unit.Id).ToList();
        if (target == OccurrenceStatus.EM_ATENDIMENTO)
        {
            if (units.Count == 0)
                throw new AppException("A ocorrência não possui equipes despachadas.", StatusCodes.Status409Conflict);
            if (units.Any(unit => unit.Status != UnitStatus.DESLOCAMENTO))
                throw new AppException("Todas as equipes despachadas devem estar em deslocamento.", StatusCodes.Status409Conflict);
            return ChangeUnits(units, UnitStatus.EM_ATENDIMENTO);
        }

        if (target == OccurrenceStatus.FINALIZADA)
        {
            if (units.Count == 0 || units.Any(unit => unit.Status != UnitStatus.EM_ATENDIMENTO))
                throw new AppException("Todas as equipes devem estar em atendimento para finalizar a ocorrência.", StatusCodes.Status409Conflict);
            return ChangeUnits(units, UnitStatus.DISPONIVEL);
        }

        if (target == OccurrenceStatus.CANCELADA && units.Count > 0)
        {
            if (units.Any(unit => unit.Status is not UnitStatus.RESERVADA and not UnitStatus.DESLOCAMENTO and not UnitStatus.EM_ATENDIMENTO))
                throw new AppException("Uma equipe relacionada está em estado incompatível com o cancelamento.", StatusCodes.Status409Conflict);
            foreach (var dispatch in occurrence.Dispatches.Where(item => item.Status is not DispatchStatus.CONCLUIDO and not DispatchStatus.CANCELADO))
            {
                dispatch.Status = DispatchStatus.CANCELADO;
                dispatch.CancelledAt = DateTime.UtcNow;
            }
            return ChangeUnits(units, UnitStatus.DISPONIVEL);
        }

        return [];
    }

    private static IReadOnlyList<OccurrenceUnitStatusChangeDto> ChangeUnits(List<Unit> units, UnitStatus target)
        => units.Select(unit =>
        {
            var previous = unit.Status;
            unit.Status = target;
            return new OccurrenceUnitStatusChangeDto
            {
                UnitId = unit.Id,
                UnitName = unit.Name,
                PreviousStatus = previous.ToString(),
                CurrentStatus = target.ToString()
            };
        }).ToList();
}
