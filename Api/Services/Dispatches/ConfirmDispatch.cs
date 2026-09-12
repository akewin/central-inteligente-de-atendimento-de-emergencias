using System.Net;
using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Models;
using Api.Models.Enums;
using Api.Security.Jwt;
using Api.Services.Occurrences;
using Api.Validations;
using Microsoft.EntityFrameworkCore;
using Api.Realtime;

namespace Api.Services.Dispatches;

public sealed class ConfirmDispatch
{
    private readonly IDispatchRepository _repository;
    private readonly DispatchValidator _validator;
    private readonly CurrentUserContext _currentUser;
    private readonly ApiDbContext _context;
    private readonly CreateSystemLog _audit;
    private readonly OccurrenceStateMachine _stateMachine;
    private readonly IDispatchRealtimeNotifier _realtime;
    private readonly ILogger<ConfirmDispatch> _logger;

    public ConfirmDispatch(
        IDispatchRepository repository,
        DispatchValidator validator,
        CurrentUserContext currentUser,
        ApiDbContext context,
        CreateSystemLog audit,
        OccurrenceStateMachine stateMachine, IDispatchRealtimeNotifier realtime, ILogger<ConfirmDispatch> logger)
        => (_repository, _validator, _currentUser, _context, _audit, _stateMachine, _realtime, _logger)
            = (repository, validator, currentUser, context, audit, stateMachine, realtime, logger);

    public async Task<DispatchConfirmationReadDto> ExecuteAsync(
        int occurrenceId,
        DispatchConfirmationRequestDto? dto,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositiveInt(occurrenceId);
        var unitIds = _validator.Validate(dto);
        var userId = _currentUser.GetId()
            ?? throw new AppException("Usuário não autenticado.", (int)HttpStatusCode.Unauthorized);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var user = await _context.Users.SingleOrDefaultAsync(
            item => item.Id == userId && item.Active,
            cancellationToken)
            ?? throw new AppException("Usuário autenticado não encontrado ou inativo.", (int)HttpStatusCode.Unauthorized);

        var occurrence = await _repository.GetOccurrenceForConfirmationAsync(occurrenceId, cancellationToken)
            ?? throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        ValidateOccurrence(occurrence);
        _stateMachine.EnsureAllowed(occurrence.Status, OccurrenceStatus.DESPACHADA);
        if (await _repository.ExistsForOccurrenceAsync(occurrenceId, cancellationToken))
            throw new AppException("A ocorrência já possui despacho registrado.", (int)HttpStatusCode.Conflict);

        var units = await _repository.GetUnitsForConfirmationAsync(unitIds, cancellationToken);
        if (units.Count != unitIds.Count)
            throw new AppException("Uma ou mais equipes selecionadas não foram encontradas.", (int)HttpStatusCode.NotFound);

        var activeUnitIds = await _context.Dispatches.AsNoTracking()
            .Where(dispatch => unitIds.Contains(dispatch.UnitId)
                && dispatch.Status != DispatchStatus.CONCLUIDO
                && dispatch.Status != DispatchStatus.CANCELADO)
            .Select(dispatch => dispatch.UnitId)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (activeUnitIds.Count > 0)
        {
            var busyNames = units.Where(unit => activeUnitIds.Contains(unit.Id)).Select(unit => unit.Name);
            throw new AppException(
                $"As seguintes equipes já possuem um chamado ativo: {string.Join(", ", busyNames)}. Atualize as recomendações e selecione outra equipe.",
                (int)HttpStatusCode.Conflict);
        }

        var confirmedServices = GetConfirmedServices(occurrence);
        ValidateUnits(units, confirmedServices);

        var unitHasActiveDispatch = await _context.Dispatches
            .AsNoTracking()
            .AnyAsync(dispatch => unitIds.Contains(dispatch.UnitId)
                && dispatch.Status != DispatchStatus.CONCLUIDO
                && dispatch.Status != DispatchStatus.CANCELADO,
                cancellationToken);
        if (unitHasActiveDispatch)
            throw new AppException(
                "Uma ou mais equipes selecionadas já possuem um chamado ativo. Atualize as recomendações e escolha outra equipe.",
                (int)HttpStatusCode.Conflict);

        var dispatches = units.Select(unit => new Dispatch
        {
            Occurrence = occurrence,
            OccurrenceId = occurrence.Id,
            Unit = unit,
            UnitId = unit.Id,
            ConfirmedByUser = user,
            ConfirmedByUserId = user.Id,
            Status = DispatchStatus.ATRIBUIDO
        }).ToList();

        foreach (var unit in units)
            unit.Status = UnitStatus.RESERVADA;
        occurrence.Status = OccurrenceStatus.DESPACHADA;
        _repository.AddRange(dispatches);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException(
                "A ocorrência ou uma das equipes foi alterada por outro usuário. Atualize os dados e tente novamente.",
                (int)HttpStatusCode.Conflict);
        }
        catch (DbUpdateException)
        {
            throw new AppException(
                "Não foi possível registrar o despacho devido a um conflito de dados.",
                (int)HttpStatusCode.Conflict);
        }

        var confirmedAt = dispatches.Min(dispatch => dispatch.CreatedAt);
        await _audit.ExecuteAsync(
            action: $"confirm Dispatch occurrence id: {occurrence.Id}",
            data: new SystemLogDataDto
            {
                Type = "create",
                Created = new
                {
                    occurrence.Id,
                    OccurrenceStatus = occurrence.Status.ToString(),
                    ConfirmedByUserId = user.Id,
                    ConfirmedAt = confirmedAt,
                    Dispatches = dispatches.Select(dispatch => new
                    {
                        dispatch.Id,
                        dispatch.UnitId,
                        UnitName = dispatch.Unit.Name,
                        Service = dispatch.Unit.EmergencyService.Type.ToString()
                    })
                }
            });
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        foreach (var dispatch in dispatches)
        {
            try
            {
                await _realtime.DispatchAssignedAsync(new DispatchRealtimeEventDto
                {
                    DispatchId = dispatch.Id, OccurrenceId = occurrence.Id, UnitId = dispatch.UnitId,
                    DispatchStatus = dispatch.Status.ToString(), OccurrenceStatus = occurrence.Status.ToString(),
                    UpdatedAt = dispatch.UpdatedAt
                }, cancellationToken);
            }
            catch (Exception error)
            {
                _logger.LogWarning(error, "Falha ao publicar atribuição do despacho {DispatchId}", dispatch.Id);
            }
        }

        return new DispatchConfirmationReadDto
        {
            OccurrenceId = occurrence.Id,
            OccurrenceStatus = occurrence.Status.ToString(),
            ConfirmedAt = confirmedAt,
            ConfirmedBy = new OccurrenceCreatorDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName
            },
            Dispatches = dispatches.Select(dispatch => new DispatchReadDto
            {
                Id = dispatch.Id,
                UnitId = dispatch.UnitId,
                UnitName = dispatch.Unit.Name,
                Service = dispatch.Unit.EmergencyService.Type.ToString(),
                UnitStatus = dispatch.Unit.Status.ToString()
                ,Status = dispatch.Status.ToString(), CreatedAt = dispatch.CreatedAt, UpdatedAt = dispatch.UpdatedAt
            }).ToList()
        };
    }

    private static void ValidateOccurrence(Occurrence occurrence)
    {
        if (occurrence.Status != OccurrenceStatus.AGUARDANDO_CONFIRMACAO)
            throw new AppException("O status atual da ocorrência não permite despacho.", (int)HttpStatusCode.Conflict);
        if (!occurrence.ServicesConfirmedAt.HasValue || !occurrence.ServicesConfirmedByUserId.HasValue)
            throw new AppException("Os órgãos da ocorrência ainda não foram confirmados.", (int)HttpStatusCode.Conflict);
    }

    private static void ValidateUnits(
        IReadOnlyCollection<Unit> units,
        IReadOnlySet<EmergencyServiceType> confirmedServices)
    {
        if (confirmedServices.Count == 0)
            throw new AppException("A ocorrência não possui órgãos confirmados.", (int)HttpStatusCode.Conflict);
        if (units.Any(unit => unit.Status != UnitStatus.DISPONIVEL))
            throw new AppException("Uma ou mais equipes selecionadas não estão disponíveis.", (int)HttpStatusCode.Conflict);

        var selectedServices = units.Select(unit => unit.EmergencyService.Type).ToHashSet();
        if (selectedServices.Count != units.Count)
            throw new AppException("Selecione exatamente uma equipe por órgão confirmado.");
        if (!selectedServices.SetEquals(confirmedServices))
            throw new AppException("As equipes devem corresponder exatamente aos órgãos confirmados.", (int)HttpStatusCode.Conflict);
    }

    private static IReadOnlySet<EmergencyServiceType> GetConfirmedServices(Occurrence occurrence)
    {
        var services = new HashSet<EmergencyServiceType>();
        if (occurrence.PoliceConfirmed) services.Add(EmergencyServiceType.POLICIA);
        if (occurrence.SamuConfirmed) services.Add(EmergencyServiceType.SAMU);
        if (occurrence.FireDepartmentConfirmed) services.Add(EmergencyServiceType.BOMBEIROS);
        return services;
    }
}
