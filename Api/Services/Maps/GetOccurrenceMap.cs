using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Geography;
using Api.Mappers;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Services.Geography;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Maps;

public sealed class GetOccurrenceMap
{
    private const int DefaultRadiusKm = 15;
    private const int MaximumRadiusKm = 50;
    private const int DefaultHospitalLimit = 8;
    private const int MaximumHospitalLimit = 15;

    private readonly ApiDbContext _context;
    private readonly IHospitalFinder _hospitalFinder;
    private readonly IMapRoutingService _routing;
    private readonly GeographicDistanceCalculator _distanceCalculator;
    private readonly GeocodingRefreshService _geocodingRefresh;

    public GetOccurrenceMap(
        ApiDbContext context,
        IHospitalFinder hospitalFinder,
        IMapRoutingService routing,
        GeographicDistanceCalculator distanceCalculator,
        GeocodingRefreshService geocodingRefresh)
        => (_context, _hospitalFinder, _routing, _distanceCalculator, _geocodingRefresh)
            = (context, hospitalFinder, routing, distanceCalculator, geocodingRefresh);

    public async Task<OccurrenceMapDto> ExecuteAsync(
        int occurrenceId,
        int radiusKm = DefaultRadiusKm,
        int hospitalLimit = DefaultHospitalLimit,
        int? hospitalId = null,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositiveInt(occurrenceId);
        if (radiusKm is < 1 or > MaximumRadiusKm)
            throw new AppException("O raio de busca deve estar entre 1 e 50 km.");
        if (hospitalLimit is < 1 or > MaximumHospitalLimit)
            throw new AppException("O limite de hospitais deve estar entre 1 e 15.");

        var occurrence = await _context.Occurrences
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == occurrenceId, cancellationToken)
            ?? throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        if (!GeographicDistanceCalculator.AreValidCoordinates(occurrence.Latitude, occurrence.Longitude))
            throw new AppException("A ocorrência possui coordenadas inválidas.", (int)HttpStatusCode.UnprocessableEntity);

        var ambulances = await _context.Dispatches
            .AsNoTracking()
            .Where(dispatch => dispatch.OccurrenceId == occurrenceId
                && dispatch.Unit.EmergencyService.Type == EmergencyServiceType.SAMU)
            .Select(dispatch => dispatch.Unit)
            .OrderBy(unit => unit.Id)
            .ToListAsync(cancellationToken);

        // Registros criados antes da geocodificação real são corrigidos na primeira consulta.
        await _geocodingRefresh.RefreshAsync(occurrence, cancellationToken);
        foreach (var ambulance in ambulances)
            await _geocodingRefresh.RefreshAsync(ambulance, cancellationToken);

        var warnings = new List<string>();
        var hospitalSearch = await LoadHospitalsAsync(
            occurrence.Latitude,
            occurrence.Longitude,
            radiusKm,
            hospitalLimit,
            warnings,
            cancellationToken);

        var hospitals = hospitalSearch.Hospitals;
        var requestedHospitalId = !hospitalId.HasValue
            ? occurrence.SelectedHospitalId
            : hospitalId;
        var selectedHospital = !requestedHospitalId.HasValue
            ? hospitals.FirstOrDefault()
            : hospitals.FirstOrDefault(item => item.Id == requestedHospitalId);
        if (hospitalId.HasValue && selectedHospital is null)
            throw new AppException("O hospital selecionado não está entre os destinos próximos.");

        var routes = new List<MapRouteDto>();
        if (ambulances.Count == 0)
        {
            warnings.Add("A rota da ambulância aparecerá após o despacho de uma equipe SAMU.");
        }
        else if (selectedHospital is not null)
        {
            foreach (var ambulance in ambulances)
            {
                try
                {
                    var route = await _routing.FindRouteAsync(
                        [
                            new MapCoordinate(ambulance.Latitude, ambulance.Longitude),
                            new MapCoordinate(occurrence.Latitude, occurrence.Longitude),
                            new MapCoordinate(selectedHospital.Latitude, selectedHospital.Longitude)
                        ],
                        cancellationToken);

                    if (route is null || route.Legs.Count < 2)
                    {
                        warnings.Add($"Não foi encontrada uma rota viária para a equipe {ambulance.Name}.");
                        continue;
                    }

                    routes.Add(new MapRouteDto
                    {
                        UnitId = ambulance.Id,
                        UnitName = ambulance.Name,
                        HospitalId = selectedHospital.Id,
                        TotalDistanceKm = Round(route.DistanceKm),
                        TotalEstimatedMinutes = Round(route.DurationMinutes),
                        UnitToVictimDistanceKm = Round(route.Legs[0].DistanceKm),
                        UnitToVictimMinutes = Round(route.Legs[0].DurationMinutes),
                        VictimToHospitalDistanceKm = Round(route.Legs[1].DistanceKm),
                        VictimToHospitalMinutes = Round(route.Legs[1].DurationMinutes),
                        Geometry = route.Geometry.Select(point => new MapCoordinateDto
                        {
                            Latitude = point.Latitude,
                            Longitude = point.Longitude
                        }).ToList()
                    });
                }
                catch (Exception error) when (IsExternalServiceFailure(error, cancellationToken))
                {
                    warnings.Add($"A rota viária da equipe {ambulance.Name} está temporariamente indisponível.");
                }
            }
        }

        return new OccurrenceMapDto
        {
            OccurrenceId = occurrence.Id,
            GeneratedAt = DateTime.UtcNow,
            Victim = new MapPointDto
            {
                Latitude = occurrence.Latitude,
                Longitude = occurrence.Longitude,
                Label = occurrence.LocationDescription
            },
            Ambulances = ambulances.Select(unit => new MapUnitDto
            {
                Id = unit.Id,
                Name = unit.Name,
                Status = unit.Status.ToString(),
                Address = UnitMapper.FormatAddress(unit),
                Latitude = unit.Latitude,
                Longitude = unit.Longitude
            }).ToList(),
            Hospitals = hospitals,
            HospitalSearchSucceeded = hospitalSearch.Succeeded,
            SelectedHospitalId = selectedHospital?.Id,
            HospitalSelectionConfirmed = selectedHospital is not null
                && selectedHospital.Id == occurrence.SelectedHospitalId,
            Routes = routes,
            Warnings = warnings.Distinct(StringComparer.Ordinal).ToList()
        };
    }

    private async Task<HospitalSearchResult> LoadHospitalsAsync(
        decimal latitude,
        decimal longitude,
        int radiusKm,
        int hospitalLimit,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        var candidates = await _context.Hospitals.AsNoTracking().Where(h => h.Active)
            .Select(h => new HospitalCandidate(h.Id.ToString(), h.Name,
                h.Street + ", " + h.Number + " - " + h.Neighborhood + " - Araras/SP",
                h.Latitude, h.Longitude, h.HasEmergencyDepartment)).ToListAsync(cancellationToken);

        var origin = new MapCoordinate(latitude, longitude);
        var nearby = candidates
            .Select(candidate => new
            {
                Candidate = candidate,
                StraightDistance = _distanceCalculator.CalculateKm(
                    latitude, longitude, candidate.Latitude, candidate.Longitude)
            })
            .Where(item => item.StraightDistance <= radiusKm)
            .OrderBy(item => item.StraightDistance)
            .ThenBy(item => item.Candidate.Name, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Min(hospitalLimit * 2, 30))
            .ToList();

        IReadOnlyList<RouteEstimate> estimates = Array.Empty<RouteEstimate>();
        try
        {
            estimates = await _routing.EstimateFromAsync(
                origin,
                nearby.Select(item => new MapCoordinate(
                    item.Candidate.Latitude,
                    item.Candidate.Longitude)).ToList(),
                cancellationToken);
        }
        catch (Exception error) when (IsExternalServiceFailure(error, cancellationToken))
        {
            warnings.Add("Os hospitais foram ordenados por distância aproximada porque o cálculo viário está indisponível.");
        }

        var mappedHospitals = nearby
            .Select((item, index) => new MapHospitalDto
            {
                Id = int.Parse(item.Candidate.Id),
                Name = item.Candidate.Name,
                Address = item.Candidate.Address,
                Latitude = item.Candidate.Latitude,
                Longitude = item.Candidate.Longitude,
                HasEmergencyDepartment = item.Candidate.HasEmergencyDepartment,
                StraightLineDistanceKm = Round(item.StraightDistance),
                RoadDistanceKm = index < estimates.Count ? NullableRound(estimates[index].DistanceKm) : null,
                EstimatedMinutes = index < estimates.Count ? NullableRound(estimates[index].DurationMinutes) : null
            })
            .ToList();
        var hospitals = HospitalRecommendationRanker.Rank(mappedHospitals, hospitalLimit).ToList();
        return new HospitalSearchResult(true, hospitals);
    }

    private static bool IsExternalServiceFailure(Exception error, CancellationToken cancellationToken)
        => error is HttpRequestException
            || error is TaskCanceledException && !cancellationToken.IsCancellationRequested;

    private static double Round(double value)
        => Math.Round(value, 1, MidpointRounding.AwayFromZero);

    private static double? NullableRound(double? value) => value.HasValue ? Round(value.Value) : null;

    private sealed record HospitalSearchResult(bool Succeeded, List<MapHospitalDto> Hospitals);
}
