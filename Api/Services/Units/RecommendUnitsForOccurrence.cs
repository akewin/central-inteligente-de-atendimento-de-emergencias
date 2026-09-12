using System.Net;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Models;
using Api.Models.Enums;
using Api.Services.Geography;

namespace Api.Services.Units;

public sealed class RecommendUnitsForOccurrence
{
    private const int DefaultLimitPerService = 3;
    private const int MaximumLimitPerService = 10;

    private readonly IOccurrenceRepository _occurrences;
    private readonly IUnitRepository _units;
    private readonly GeographicDistanceCalculator _distanceCalculator;
    private readonly GeocodingRefreshService? _geocodingRefresh;

    public RecommendUnitsForOccurrence(
        IOccurrenceRepository occurrences,
        IUnitRepository units,
        GeographicDistanceCalculator distanceCalculator,
        GeocodingRefreshService geocodingRefresh)
        => (_occurrences, _units, _distanceCalculator, _geocodingRefresh)
            = (occurrences, units, distanceCalculator, geocodingRefresh);

    // Mantém os testes unitários e consumidores internos independentes de infraestrutura externa.
    public RecommendUnitsForOccurrence(
        IOccurrenceRepository occurrences,
        IUnitRepository units,
        GeographicDistanceCalculator distanceCalculator)
        => (_occurrences, _units, _distanceCalculator) = (occurrences, units, distanceCalculator);

    public async Task<OccurrenceUnitRecommendationsDto> ExecuteAsync(
        int occurrenceId,
        int limitPerService = DefaultLimitPerService,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositiveInt(occurrenceId);
        if (limitPerService is < 1 or > MaximumLimitPerService)
            throw new AppException("O limite por serviço deve estar entre 1 e 10.");

        var occurrence = await _occurrences.GetForUnitRecommendationAsync(occurrenceId, cancellationToken)
            ?? throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        EnsureOccurrenceAllowsRecommendation(occurrence);
        var confirmedServices = GetConfirmedServices(occurrence);
        if (confirmedServices.Count == 0)
            throw new AppException("A ocorrência não possui órgãos confirmados.", (int)HttpStatusCode.Conflict);

        var services = await _units.GetServicesAsync(confirmedServices, cancellationToken);
        if (services.Count != confirmedServices.Count)
            throw new AppException("Um dos serviços confirmados não está configurado.", (int)HttpStatusCode.Conflict);

        var availableUnits = await _units.GetAvailableByServicesAsync(confirmedServices, cancellationToken);
        if (_geocodingRefresh is not null)
        {
            await _geocodingRefresh.RefreshAsync(occurrence, cancellationToken);
            foreach (var unit in availableUnits)
                await _geocodingRefresh.RefreshAsync(unit, cancellationToken);
        }
        var groups = services
            .OrderBy(service => Array.IndexOf(ConfirmedServiceOrder, service.Type))
            .Select(service => MapGroup(service, availableUnits, occurrence, limitPerService))
            .ToList();

        return new OccurrenceUnitRecommendationsDto
        {
            OccurrenceId = occurrence.Id,
            GeneratedAt = DateTime.UtcNow,
            Groups = groups
        };
    }

    private UnitRecommendationGroupDto MapGroup(
        EmergencyService service,
        IEnumerable<Unit> units,
        Occurrence occurrence,
        int limit)
    {
        var recommendations = units
            .Where(unit => unit.Status == UnitStatus.DISPONIVEL && unit.EmergencyService.Type == service.Type)
            .Select(unit => new
            {
                Unit = unit,
                Distance = _distanceCalculator.CalculateKm(
                    occurrence.Latitude, occurrence.Longitude, unit.Latitude, unit.Longitude)
            })
            .OrderBy(item => item.Distance)
            .ThenBy(item => item.Unit.NormalizedName, StringComparer.Ordinal)
            .ThenBy(item => item.Unit.Id)
            .Take(limit)
            .Select(item => new UnitRecommendationDto
            {
                Id = item.Unit.Id,
                Name = item.Unit.Name,
                Status = item.Unit.Status.ToString(),
                Address = Api.Mappers.UnitMapper.FormatAddress(item.Unit),
                DistanceKm = Math.Round(item.Distance, 3, MidpointRounding.AwayFromZero)
            })
            .ToList();

        return new UnitRecommendationGroupDto
        {
            Service = service.Type.ToString(),
            EmergencyNumber = service.EmergencyNumber,
            DisplayName = service.DisplayName,
            Units = recommendations
        };
    }

    private static void EnsureOccurrenceAllowsRecommendation(Occurrence occurrence)
    {
        if (!GeographicDistanceCalculator.AreValidCoordinates(occurrence.Latitude, occurrence.Longitude))
            throw new AppException("A ocorrência possui coordenadas inválidas.", (int)HttpStatusCode.UnprocessableEntity);

        if (occurrence.Status != OccurrenceStatus.AGUARDANDO_CONFIRMACAO)
            throw new AppException(
                "O status atual da ocorrência não permite recomendar equipes.",
                (int)HttpStatusCode.Conflict);

        if (!occurrence.ServicesConfirmedAt.HasValue || !occurrence.ServicesConfirmedByUserId.HasValue)
            throw new AppException("Os órgãos da ocorrência ainda não foram confirmados.", (int)HttpStatusCode.Conflict);
    }

    private static List<EmergencyServiceType> GetConfirmedServices(Occurrence occurrence)
    {
        var services = new List<EmergencyServiceType>(3);
        if (occurrence.PoliceConfirmed) services.Add(EmergencyServiceType.POLICIA);
        if (occurrence.SamuConfirmed) services.Add(EmergencyServiceType.SAMU);
        if (occurrence.FireDepartmentConfirmed) services.Add(EmergencyServiceType.BOMBEIROS);
        return services;
    }

    private static readonly EmergencyServiceType[] ConfirmedServiceOrder =
        [EmergencyServiceType.POLICIA, EmergencyServiceType.SAMU, EmergencyServiceType.BOMBEIROS];
}
