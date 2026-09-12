using System.Data;
using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Realtime;
using Api.Security.Jwt;
using Api.Security.Permissions;
using Api.Services.UnitOperations;
using Microsoft.EntityFrameworkCore;
using Api.Auditing;
using Api.Auditing.Services;

namespace Api.Services.Hospitals;

public sealed class HospitalReception
{
    private readonly ApiDbContext _context;
    private readonly CurrentUserContext _current;
    private readonly IDispatchRealtimeNotifier _realtime;
    private readonly CreateSystemLog _audit;
    public HospitalReception(ApiDbContext context, CurrentUserContext current, IDispatchRealtimeNotifier realtime, CreateSystemLog audit)
        => (_context, _current, _realtime, _audit) = (context, current, realtime, audit);

    public async Task<IReadOnlyList<PatientTransportDto>> ListAsync(string scope, CancellationToken ct)
    {
        var hospitalId = await ResolveHospital(ct);
        var query = _context.PatientTransports.AsNoTracking().IncludeAll().Where(t => t.HospitalId == hospitalId);
        query = scope.Equals("history", StringComparison.OrdinalIgnoreCase)
            ? query.Where(t => t.Status == TransportStatus.RECEBIDO || t.Status == TransportStatus.CANCELADO)
            : query.Where(t => t.Status != TransportStatus.RECEBIDO && t.Status != TransportStatus.CANCELADO);
        return (await query.OrderByDescending(t => t.CreatedAt).ToListAsync(ct)).Select(HospitalMapper.ToDto).ToList();
    }

    public async Task<HospitalDto> GetHospitalAsync(CancellationToken ct)
    {
        var id = await ResolveHospital(ct);
        var hospital = await _context.Hospitals.AsNoTracking().Include(h => h.Wards).SingleAsync(h => h.Id == id, ct);
        return HospitalMapper.ToDto(hospital);
    }

    public async Task<HospitalWardDto> UpdateWardAsync(int wardId, HospitalWardSaveDto dto, CancellationToken ct)
    {
        var hospitalId = await ResolveHospital(ct);
        var ward = await _context.HospitalWards.SingleOrDefaultAsync(w => w.Id == wardId && w.HospitalId == hospitalId, ct)
            ?? throw new AppException("Ala não encontrada.", 404);
        if (!dto.ExpectedUpdatedAt.HasValue) throw new AppException("Versão da ala não informada.");
        if (dto.TotalBeds < 0 || dto.OccupiedBeds < 0 || dto.OccupiedBeds + ward.ReservedBeds > dto.TotalBeds)
            throw new AppException("A capacidade não comporta as vagas ocupadas e reservadas.", 409);
        _context.Entry(ward).Property(w => w.UpdatedAt).OriginalValue = dto.ExpectedUpdatedAt.Value;
        ward.TotalBeds = dto.TotalBeds; ward.OccupiedBeds = dto.OccupiedBeds; ward.Active = dto.Active;
        try { await _context.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new AppException("A ala foi atualizada em outro dispositivo.", 409); }
        return HospitalMapper.ToDto(ward);
    }

    public async Task<PatientTransportDto> ActAsync(int id, HospitalReceptionActionDto dto, CancellationToken ct)
    {
        var hospitalId = await ResolveHospital(ct);
        await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var transport = await _context.PatientTransports.IncludeAll().SingleOrDefaultAsync(t => t.Id == id && t.HospitalId == hospitalId, ct)
            ?? throw new AppException("Recebimento não encontrado.", 404);
        if (dto.ExpectedUpdatedAt.HasValue) _context.Entry(transport).Property(t => t.UpdatedAt).OriginalValue = dto.ExpectedUpdatedAt.Value;
        var action = dto.Action.Trim().ToUpperInvariant();
        if (action == "CIENCIA" && transport.Status == TransportStatus.HOSPITAL_AVISADO && !transport.AcknowledgedAt.HasValue)
            transport.AcknowledgedAt = DateTime.UtcNow;
        else if (action == "RECEBIDO" && transport.Status is TransportStatus.HOSPITAL_AVISADO or TransportStatus.EM_TRANSPORTE)
        {
            var ward = await _context.HospitalWards.SingleAsync(w => w.Id == transport.HospitalWardId, ct);
            if (ward.ReservedBeds <= 0 || ward.OccupiedBeds >= ward.TotalBeds) throw new AppException("A reserva da vaga não está mais disponível.", 409);
            ward.ReservedBeds--; ward.OccupiedBeds++; transport.Status = TransportStatus.RECEBIDO; transport.ReceivedAt = DateTime.UtcNow;
            if (transport.SamuDispatchId.HasValue)
            {
                var samu = await _context.Dispatches.Include(d => d.Unit).SingleAsync(d => d.Id == transport.SamuDispatchId, ct);
                samu.Status = DispatchStatus.CONCLUIDO; samu.CompletedAt = DateTime.UtcNow; samu.Unit.Status = UnitStatus.DISPONIVEL;
                var otherActive = await _context.Dispatches.AnyAsync(d => d.OccurrenceId == transport.OccurrenceId && d.Id != samu.Id
                    && d.Status != DispatchStatus.CONCLUIDO && d.Status != DispatchStatus.CANCELADO, ct);
                if (!otherActive) transport.Occurrence.Status = OccurrenceStatus.FINALIZADA;
            }
        }
        else if (action == "CANCELADO" && transport.Status is not TransportStatus.RECEBIDO and not TransportStatus.CANCELADO)
        {
            if (transport.HospitalWardId.HasValue)
            {
                var ward = await _context.HospitalWards.SingleAsync(w => w.Id == transport.HospitalWardId, ct);
                ward.ReservedBeds = Math.Max(0, ward.ReservedBeds - 1);
            }
            transport.Status = TransportStatus.CANCELADO; transport.CancelledAt = DateTime.UtcNow;
        }
        else throw new AppException("A ação não é compatível com o estado atual do recebimento.", 409);
        try { await _context.SaveChangesAsync(ct); await tx.CommitAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new AppException("O recebimento foi atualizado em outro dispositivo.", 409); }
        await _audit.ExecuteAsync(SystemLogActionFactory.Update("PatientTransport", transport.Id), data: new { Action = action, Status = transport.Status.ToString(), transport.HospitalId });
        await _realtime.TransportChangedAsync(HospitalMapper.ToDto(transport), ct);
        return HospitalMapper.ToDto(transport);
    }

    private async Task<int> ResolveHospital(CancellationToken ct)
    {
        var userId = _current.GetId() ?? throw new AppException("Usuário não autenticado.", 401);
        var user = await _context.Users.AsNoTracking().Include(u => u.AccessPermissions).SingleOrDefaultAsync(u => u.Id == userId && u.Active, ct)
            ?? throw new AppException("Conta hospitalar não encontrada.", 401);
        if (!user.HospitalId.HasValue || !user.AccessPermissions.Any(p => p.SystemResourceId == BasePermissions.HOSPITAL_OPERATIONS))
            throw new AppException("A conta não possui vínculo hospitalar válido.", 403);
        return user.HospitalId.Value;
    }
}
