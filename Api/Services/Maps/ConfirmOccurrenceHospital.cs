using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Maps;

public sealed class ConfirmOccurrenceHospital
{
    private readonly ApiDbContext _context;
    private readonly GetOccurrenceMap _map;

    public ConfirmOccurrenceHospital(ApiDbContext context, GetOccurrenceMap map)
        => (_context, _map) = (context, map);

    public async Task<HospitalSelectionReadDto> ExecuteAsync(
        int occurrenceId, HospitalSelectionRequestDto? request, CancellationToken cancellationToken)
    {
        throw new AppException("O destino agora deve ser definido pela equipe SAMU no local, com a seleção de uma ala.", (int)HttpStatusCode.Conflict);
#pragma warning disable CS0162
        if (occurrenceId <= 0 || request?.HospitalId <= 0)
            throw new AppException("Selecione um hospital válido.");

        var hospitalId = request!.HospitalId;
        var map = await _map.ExecuteAsync(occurrenceId, hospitalId: hospitalId, cancellationToken: cancellationToken);
        var hospital = map.Hospitals.Single(item => item.Id == hospitalId);
        var occurrence = await _context.Occurrences.SingleOrDefaultAsync(item => item.Id == occurrenceId, cancellationToken)
            ?? throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);
        var now = DateTime.UtcNow;
        occurrence.SelectedHospitalId = hospital.Id;
        occurrence.SelectedHospitalName = hospital.Name;
        occurrence.SelectedHospitalAddress = hospital.Address;
        occurrence.SelectedHospitalLatitude = hospital.Latitude;
        occurrence.SelectedHospitalLongitude = hospital.Longitude;
        occurrence.HospitalSelectedAt = now;
        await _context.SaveChangesAsync(cancellationToken);

        return new HospitalSelectionReadDto
        {
            OccurrenceId = occurrence.Id,
            HospitalId = hospital.Id,
            HospitalName = hospital.Name,
            HospitalAddress = hospital.Address,
            SelectedAt = now
        };
#pragma warning restore CS0162
    }
}
