using System.Net;
using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Security.Jwt;
using Api.Validations;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Occurrences;

public sealed class ConfirmOccurrenceServices
{
    private readonly IOccurrenceRepository _occurrences;
    private readonly OccurrenceServiceConfirmationValidator _validator;
    private readonly CurrentUserContext _currentUser;
    private readonly ApiDbContext _context;
    private readonly CreateSystemLog _createSystemLog;

    public ConfirmOccurrenceServices(
        IOccurrenceRepository occurrences,
        OccurrenceServiceConfirmationValidator validator,
        CurrentUserContext currentUser,
        ApiDbContext context,
        CreateSystemLog createSystemLog)
    {
        _occurrences = occurrences;
        _validator = validator;
        _currentUser = currentUser;
        _context = context;
        _createSystemLog = createSystemLog;
    }

    public async Task<OccurrenceServiceConfirmationReadDto> ExecuteAsync(
        int occurrenceId,
        OccurrenceServiceConfirmationRequestDto? dto,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositiveInt(occurrenceId);
        var selected = _validator.Validate(dto);
        var occurrence = await _occurrences.GetForServiceConfirmationAsync(
            occurrenceId,
            cancellationToken)
            ?? throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        if (occurrence.Status != OccurrenceStatus.AGUARDANDO_CONFIRMACAO)
            throw new AppException(
                "O status atual da ocorrência não permite confirmar órgãos.",
                (int)HttpStatusCode.Conflict);

        var latestAnalysis = occurrence.AIAnalyses
            .OrderByDescending(a => a.CreatedAt)
            .ThenByDescending(a => a.Id)
            .FirstOrDefault()
            ?? throw new AppException(
                "A ocorrência não possui recomendação de IA para confirmação.",
                (int)HttpStatusCode.Conflict);

        if (occurrence.ServicesConfirmedAt.HasValue)
        {
            if (MatchesExistingDecision(occurrence, selected))
                return OccurrenceServiceConfirmationMapper.ToReadDto(occurrence)!;

            throw new AppException(
                "Os órgãos desta ocorrência já foram confirmados.",
                (int)HttpStatusCode.Conflict);
        }

        var userId = _currentUser.GetId()
            ?? throw new AppException("Usuário não autenticado.", (int)HttpStatusCode.Unauthorized);
        var user = await _context.Users.SingleOrDefaultAsync(
            u => u.Id == userId && u.Active,
            cancellationToken)
            ?? throw new AppException("Usuário autenticado não encontrado ou inativo.",
                (int)HttpStatusCode.Unauthorized);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        occurrence.PoliceConfirmed = selected.Contains(EmergencyServiceType.POLICIA);
        occurrence.SamuConfirmed = selected.Contains(EmergencyServiceType.SAMU);
        occurrence.FireDepartmentConfirmed = selected.Contains(EmergencyServiceType.BOMBEIROS);
        occurrence.ServicesConfirmedByUserId = user.Id;
        occurrence.ServicesConfirmedByUser = user;
        occurrence.ServicesConfirmedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        await _context.Entry(occurrence).ReloadAsync(cancellationToken);
        occurrence.ServicesConfirmedByUser = user;

        var confirmation = OccurrenceServiceConfirmationMapper.ToReadDto(occurrence)!;
        await _createSystemLog.ExecuteAsync(
            action: SystemLogActionFactory.Update("OccurrenceServiceConfirmation", occurrence.Id),
            data: new SystemLogDataDto
            {
                Type = "update",
                CurrState = new
                {
                    occurrence.Id,
                    LatestAIAnalysisId = latestAnalysis.Id,
                    confirmation.ConfirmedServices,
                    ServicesConfirmedByUserId = user.Id,
                    confirmation.ConfirmedAt
                }
            });

        await transaction.CommitAsync(cancellationToken);
        return confirmation;
    }

    private static bool MatchesExistingDecision(
        Api.Models.Occurrence occurrence,
        IReadOnlySet<EmergencyServiceType> selected)
    {
        return occurrence.PoliceConfirmed == selected.Contains(EmergencyServiceType.POLICIA) &&
               occurrence.SamuConfirmed == selected.Contains(EmergencyServiceType.SAMU) &&
               occurrence.FireDepartmentConfirmed == selected.Contains(EmergencyServiceType.BOMBEIROS);
    }
}
