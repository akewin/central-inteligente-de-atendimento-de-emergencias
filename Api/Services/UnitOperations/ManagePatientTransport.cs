using System.Data;
using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Api.Models;
using Api.Models.Enums;
using Api.Realtime;
using Api.Services.Hospitals;
using Api.Geography;
using Api.Auditing;
using Api.Auditing.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UnitOperations;

public sealed class ManagePatientTransport
{
    private readonly ApiDbContext _context;
    private readonly OperationalUserResolver _operationalUser;
    private readonly IDispatchRealtimeNotifier _realtime;
    private readonly IMapRoutingService _routing;
    private readonly CreateSystemLog _audit;
    public ManagePatientTransport(ApiDbContext context, OperationalUserResolver operationalUser, IDispatchRealtimeNotifier realtime, IMapRoutingService routing, CreateSystemLog audit)
        => (_context, _operationalUser, _realtime, _routing, _audit) = (context, operationalUser, realtime, routing, audit);

    public async Task<PatientTransportDto> RequestAsync(int dispatchId, RequestTransportDto dto, CancellationToken ct)
    {
        var user = await _operationalUser.ResolveAsync(ct);
        var dispatch = await LoadDispatch(dispatchId, user.UnitId!.Value, ct);
        if (dispatch.Status is not DispatchStatus.NO_LOCAL and not DispatchStatus.EM_ATENDIMENTO)
            throw new AppException("Informe a chegada ao local antes de solicitar encaminhamento.", (int)HttpStatusCode.Conflict);
        var existing = await _context.PatientTransports.IncludeAll().SingleOrDefaultAsync(t => t.OccurrenceId == dispatch.OccurrenceId
            && t.Status != TransportStatus.RECEBIDO && t.Status != TransportStatus.CANCELADO, ct);
        if (existing is not null) return HospitalMapper.ToDto(existing);
        var samu = dispatch.Unit.EmergencyService.Type == EmergencyServiceType.SAMU ? dispatch :
            await _context.Dispatches.Include(d => d.Unit).ThenInclude(u => u.EmergencyService)
                .FirstOrDefaultAsync(d => d.OccurrenceId == dispatch.OccurrenceId && d.Unit.EmergencyService.Type == EmergencyServiceType.SAMU
                    && d.Status != DispatchStatus.CONCLUIDO && d.Status != DispatchStatus.CANCELADO, ct);
        var transport = new PatientTransport
        {
            OccurrenceId = dispatch.OccurrenceId, RequestedByDispatchId = dispatch.Id, SamuDispatchId = samu?.Id,
            Status = samu is null ? TransportStatus.AGUARDANDO_CENTRAL : TransportStatus.AGUARDANDO_DESTINO,
            OperationalNotes = Clean(dto.OperationalNotes)
        };
        _context.PatientTransports.Add(transport);
        await _context.SaveChangesAsync(ct);
        transport = await _context.PatientTransports.IncludeAll().SingleAsync(t => t.Id == transport.Id, ct);
        await _audit.ExecuteAsync(SystemLogActionFactory.Create("PatientTransport", transport.Id), data: new { transport.OccurrenceId, transport.RequestedByDispatchId, Status = transport.Status.ToString() });
        await _realtime.TransportChangedAsync(HospitalMapper.ToDto(transport), ct);
        return HospitalMapper.ToDto(transport);
    }

    public async Task<PatientTransportDto> AssignDestinationAsync(int dispatchId, AssignTransportDestinationDto dto, CancellationToken ct)
    {
        var user = await _operationalUser.ResolveAsync(ct);
        var dispatch = await LoadDispatch(dispatchId, user.UnitId!.Value, ct);
        if (dispatch.Unit.EmergencyService.Type != EmergencyServiceType.SAMU || dispatch.Status is not DispatchStatus.NO_LOCAL and not DispatchStatus.EM_ATENDIMENTO)
            throw new AppException("Somente uma equipe SAMU no local pode definir o destino.", (int)HttpStatusCode.Forbidden);
        await using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var transport = await _context.PatientTransports.IncludeAll().SingleOrDefaultAsync(t => t.OccurrenceId == dispatch.OccurrenceId
            && t.Status != TransportStatus.RECEBIDO && t.Status != TransportStatus.CANCELADO, ct);
        if (transport is null)
        {
            transport = new PatientTransport { OccurrenceId = dispatch.OccurrenceId, RequestedByDispatchId = dispatch.Id,
                SamuDispatchId = dispatch.Id, Status = TransportStatus.AGUARDANDO_DESTINO };
            _context.PatientTransports.Add(transport);
        }
        else if (transport.Status is TransportStatus.EM_TRANSPORTE)
            throw new AppException("O destino não pode ser alterado após o início do transporte.", (int)HttpStatusCode.Conflict);
        if (dto.ExpectedUpdatedAt.HasValue && transport.Id > 0)
            _context.Entry(transport).Property(t => t.UpdatedAt).OriginalValue = dto.ExpectedUpdatedAt.Value;
        var ward = await _context.HospitalWards.Include(w => w.Hospital).SingleOrDefaultAsync(w => w.Id == dto.HospitalWardId
            && w.HospitalId == dto.HospitalId && w.Active && w.Hospital.Active, ct)
            ?? throw new AppException("Hospital ou ala indisponível.", (int)HttpStatusCode.NotFound);
        if (ward.AvailableBeds <= 0) throw new AppException("A ala selecionada não possui vagas disponíveis.", (int)HttpStatusCode.Conflict);
        if (transport.HospitalWardId.HasValue && transport.HospitalWardId != ward.Id)
        {
            var oldWard = await _context.HospitalWards.SingleAsync(w => w.Id == transport.HospitalWardId, ct);
            oldWard.ReservedBeds = Math.Max(0, oldWard.ReservedBeds - 1);
        }
        if (transport.HospitalWardId != ward.Id) ward.ReservedBeds++;
        var now = DateTime.UtcNow;
        transport.SamuDispatchId = dispatch.Id; transport.HospitalId = ward.HospitalId; transport.HospitalWardId = ward.Id;
        transport.OperationalNotes = Clean(dto.OperationalNotes) ?? transport.OperationalNotes;
        transport.Status = TransportStatus.HOSPITAL_AVISADO; transport.HospitalNotifiedAt = now;
        try
        {
            var route = await _routing.FindRouteAsync([new MapCoordinate(dispatch.Occurrence.Latitude, dispatch.Occurrence.Longitude), new MapCoordinate(ward.Hospital.Latitude, ward.Hospital.Longitude)], ct);
            transport.EstimatedMinutes = route is null ? null : Math.Round(route.DurationMinutes, 1);
        }
        catch (HttpRequestException) { transport.EstimatedMinutes = null; }
        dispatch.Occurrence.SelectedHospitalId = ward.HospitalId; dispatch.Occurrence.SelectedHospitalName = ward.Hospital.Name;
        dispatch.Occurrence.SelectedHospitalAddress = HospitalMapper.Address(ward.Hospital);
        dispatch.Occurrence.SelectedHospitalLatitude = ward.Hospital.Latitude; dispatch.Occurrence.SelectedHospitalLongitude = ward.Hospital.Longitude;
        dispatch.Occurrence.HospitalSelectedAt = now;
        try { await _context.SaveChangesAsync(ct); await tx.CommitAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw new AppException("A capacidade mudou. Atualize os hospitais e escolha novamente.", (int)HttpStatusCode.Conflict); }
        catch (DbUpdateException) { throw new AppException("A última vaga acabou de ser reservada. Escolha outro destino.", (int)HttpStatusCode.Conflict); }
        transport = await _context.PatientTransports.IncludeAll().SingleAsync(t => t.Id == transport.Id, ct);
        await _audit.ExecuteAsync(SystemLogActionFactory.Update("PatientTransport", transport.Id), data: new { transport.HospitalId, transport.HospitalWardId, Status = transport.Status.ToString() });
        await _realtime.TransportChangedAsync(HospitalMapper.ToDto(transport), ct);
        return HospitalMapper.ToDto(transport);
    }

    public async Task<PatientTransportDto> StartAsync(int dispatchId, CancellationToken ct)
    {
        var user = await _operationalUser.ResolveAsync(ct);
        var dispatch = await LoadDispatch(dispatchId, user.UnitId!.Value, ct);
        if (dispatch.Unit.EmergencyService.Type != EmergencyServiceType.SAMU) throw new AppException("Somente o SAMU pode iniciar o transporte.", 403);
        var transport = await _context.PatientTransports.IncludeAll().SingleOrDefaultAsync(t => t.OccurrenceId == dispatch.OccurrenceId && t.Status == TransportStatus.HOSPITAL_AVISADO, ct)
            ?? throw new AppException("Defina o hospital antes de iniciar o transporte.", 409);
        transport.Status = TransportStatus.EM_TRANSPORTE; transport.TransportStartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct); await _realtime.TransportChangedAsync(HospitalMapper.ToDto(transport), ct);
        await _audit.ExecuteAsync(SystemLogActionFactory.Update("PatientTransport", transport.Id), data: new { Status = transport.Status.ToString() });
        return HospitalMapper.ToDto(transport);
    }

    public async Task<PatientTransportDto> CompleteOnSiteAsync(int dispatchId, CancellationToken ct)
    {
        var user = await _operationalUser.ResolveAsync(ct);
        var dispatch = await LoadDispatch(dispatchId, user.UnitId!.Value, ct);
        if (dispatch.Status is not DispatchStatus.NO_LOCAL and not DispatchStatus.EM_ATENDIMENTO)
            throw new AppException("A equipe precisa estar no local para encerrar sua atuação.", 409);
        dispatch.Status = DispatchStatus.CONCLUIDO; dispatch.CompletedAt = DateTime.UtcNow; dispatch.Unit.Status = UnitStatus.DISPONIVEL;
        var pending = await _context.PatientTransports.AnyAsync(t => t.OccurrenceId == dispatch.OccurrenceId && t.Status != TransportStatus.RECEBIDO && t.Status != TransportStatus.CANCELADO, ct);
        var otherActive = await _context.Dispatches.AnyAsync(d => d.OccurrenceId == dispatch.OccurrenceId && d.Id != dispatch.Id && d.Status != DispatchStatus.CONCLUIDO && d.Status != DispatchStatus.CANCELADO, ct);
        if (!pending && !otherActive) dispatch.Occurrence.Status = OccurrenceStatus.FINALIZADA;
        await _context.SaveChangesAsync(ct);
        await _audit.ExecuteAsync(SystemLogActionFactory.Update("Dispatch", dispatch.Id), data: new { Action = "complete-on-site", dispatch.OccurrenceId });
        return await CurrentForOccurrence(dispatch.OccurrenceId, ct) ?? new PatientTransportDto { OccurrenceId = dispatch.OccurrenceId, Status = "SEM_ENCAMINHAMENTO" };
    }

    private async Task<Dispatch> LoadDispatch(int id, int unitId, CancellationToken ct) => await _context.Dispatches
        .Include(d => d.Unit).ThenInclude(u => u.EmergencyService).Include(d => d.Occurrence)
        .SingleOrDefaultAsync(d => d.Id == id && d.UnitId == unitId, ct) ?? throw new AppException("Chamado não encontrado.", 404);
    private async Task<PatientTransportDto?> CurrentForOccurrence(int occurrenceId, CancellationToken ct)
        => (await _context.PatientTransports.IncludeAll().OrderByDescending(t => t.Id).FirstOrDefaultAsync(t => t.OccurrenceId == occurrenceId, ct)) is { } t ? HospitalMapper.ToDto(t) : null;
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

internal static class PatientTransportQueryExtensions
{
    public static IQueryable<PatientTransport> IncludeAll(this IQueryable<PatientTransport> query) => query
        .Include(t => t.Occurrence).Include(t => t.Hospital).Include(t => t.HospitalWard).Include(t => t.SamuDispatch);
}
