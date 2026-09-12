using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Api.Models;
using Api.Models.Enums;
using Api.Realtime;
using Api.Security.Jwt;
using Api.Services.UnitOperations;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Hospitals;

public sealed class CentralTransport
{
    private readonly ApiDbContext _context; private readonly CurrentUserContext _current; private readonly IDispatchRealtimeNotifier _realtime;
    public CentralTransport(ApiDbContext context, CurrentUserContext current, IDispatchRealtimeNotifier realtime) => (_context, _current, _realtime) = (context, current, realtime);

    public async Task<PatientTransportDto?> GetAsync(int occurrenceId, CancellationToken ct)
    {
        var transport = await _context.PatientTransports.AsNoTracking().IncludeAll().OrderByDescending(t => t.Id).FirstOrDefaultAsync(t => t.OccurrenceId == occurrenceId, ct);
        return transport is null ? null : HospitalMapper.ToDto(transport);
    }

    public async Task<PatientTransportDto> AssignSamuAsync(int occurrenceId, AssignSamuDto dto, CancellationToken ct)
    {
        var userId = _current.GetId() ?? throw new AppException("Usuário não autenticado.", 401);
        await using var tx = await _context.Database.BeginTransactionAsync(ct);
        var transport = await _context.PatientTransports.IncludeAll().SingleOrDefaultAsync(t => t.OccurrenceId == occurrenceId && t.Status == TransportStatus.AGUARDANDO_CENTRAL, ct)
            ?? throw new AppException("Não existe solicitação aguardando SAMU nesta ocorrência.", 409);
        var unit = await _context.Units.Include(u => u.EmergencyService).SingleOrDefaultAsync(u => u.Id == dto.UnitId, ct)
            ?? throw new AppException("Equipe SAMU não encontrada.", 404);
        if (unit.EmergencyService.Type != EmergencyServiceType.SAMU || unit.Status != UnitStatus.DISPONIVEL
            || await _context.Dispatches.AnyAsync(d => d.UnitId == unit.Id && d.Status != DispatchStatus.CONCLUIDO && d.Status != DispatchStatus.CANCELADO, ct))
            throw new AppException("A equipe SAMU não está disponível.", 409);
        var dispatch = new Dispatch { OccurrenceId = occurrenceId, UnitId = unit.Id, ConfirmedByUserId = userId, Status = DispatchStatus.ATRIBUIDO };
        _context.Dispatches.Add(dispatch); unit.Status = UnitStatus.RESERVADA; transport.SamuDispatch = dispatch;
        transport.Status = TransportStatus.AGUARDANDO_DESTINO; transport.Occurrence.SamuConfirmed = true;
        await _context.SaveChangesAsync(ct); await tx.CommitAsync(ct);
        await _realtime.DispatchAssignedAsync(new DispatchRealtimeEventDto { DispatchId = dispatch.Id, OccurrenceId = occurrenceId, UnitId = unit.Id,
            DispatchStatus = dispatch.Status.ToString(), OccurrenceStatus = transport.Occurrence.Status.ToString(), UpdatedAt = dispatch.UpdatedAt }, ct);
        await _realtime.TransportChangedAsync(HospitalMapper.ToDto(transport), ct);
        return HospitalMapper.ToDto(transport);
    }
}
