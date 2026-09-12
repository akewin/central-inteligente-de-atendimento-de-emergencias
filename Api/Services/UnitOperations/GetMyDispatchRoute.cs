using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Geography;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Services.Geography;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UnitOperations;

public sealed class GetMyDispatchRoute
{
    private readonly ApiDbContext _context;
    private readonly OperationalUserResolver _operationalUser;
    private readonly IMapRoutingService _routing;
    private readonly GeocodingRefreshService _geocodingRefresh;

    public GetMyDispatchRoute(ApiDbContext context, OperationalUserResolver operationalUser,
        IMapRoutingService routing, GeocodingRefreshService geocodingRefresh)
        => (_context, _operationalUser, _routing, _geocodingRefresh)
            = (context, operationalUser, routing, geocodingRefresh);

    public async Task<OperationalRouteReadDto> ExecuteAsync(int dispatchId, CancellationToken cancellationToken)
    {
        if (dispatchId <= 0) throw new AppException("Identificador inválido.");
        var user = await _operationalUser.ResolveAsync(cancellationToken);
        var dispatch = await _context.Dispatches.AsNoTracking()
            .Include(item => item.Unit).ThenInclude(unit => unit.EmergencyService)
            .Include(item => item.Occurrence)
            .SingleOrDefaultAsync(item => item.Id == dispatchId && item.UnitId == user.UnitId, cancellationToken)
            ?? throw new AppException("Chamado não encontrado para a equipe autenticada.", (int)HttpStatusCode.NotFound);

        if (dispatch.Status == DispatchStatus.ATRIBUIDO)
            throw new AppException("Aceite o chamado para visualizar a rota.", (int)HttpStatusCode.Conflict);

        await _geocodingRefresh.RefreshAsync(dispatch.Unit, cancellationToken);
        await _geocodingRefresh.RefreshAsync(dispatch.Occurrence, cancellationToken);

        if (!GeographicDistanceCalculator.AreRoutableCoordinates(
                dispatch.Unit.Latitude, dispatch.Unit.Longitude))
            throw new AppException("A equipe não possui uma localização viária válida. Atualize o endereço da equipe.", 422);
        if (!GeographicDistanceCalculator.AreRoutableCoordinates(
                dispatch.Occurrence.Latitude, dispatch.Occurrence.Longitude))
            throw new AppException(
                "Esta ocorrência antiga não possui endereço ou coordenadas válidas para traçar a rota. Registre um endereço real na ocorrência.",
                422);

        var goToHospital = dispatch.Status is DispatchStatus.NO_LOCAL or DispatchStatus.EM_ATENDIMENTO
            && dispatch.Unit.EmergencyService.Type == EmergencyServiceType.SAMU;
        if (goToHospital && (!dispatch.Occurrence.SelectedHospitalLatitude.HasValue
                || !dispatch.Occurrence.SelectedHospitalLongitude.HasValue))
            throw new AppException("A central ainda não confirmou o hospital de destino.", (int)HttpStatusCode.Conflict);
        if (goToHospital && !GeographicDistanceCalculator.AreRoutableCoordinates(
                dispatch.Occurrence.SelectedHospitalLatitude!.Value,
                dispatch.Occurrence.SelectedHospitalLongitude!.Value))
            throw new AppException("O hospital selecionado não possui coordenadas viárias válidas.", 422);

        var origin = goToHospital
            ? new MapCoordinate(dispatch.Occurrence.Latitude, dispatch.Occurrence.Longitude)
            : new MapCoordinate(dispatch.Unit.Latitude, dispatch.Unit.Longitude);
        var destination = goToHospital
            ? new MapCoordinate(dispatch.Occurrence.SelectedHospitalLatitude!.Value, dispatch.Occurrence.SelectedHospitalLongitude!.Value)
            : new MapCoordinate(dispatch.Occurrence.Latitude, dispatch.Occurrence.Longitude);
        var route = await _routing.FindRouteAsync([origin, destination], cancellationToken)
            ?? throw new AppException("Não foi possível encontrar uma rota viária para o destino.", 422);

        return new OperationalRouteReadDto
        {
            DispatchId = dispatch.Id,
            Stage = goToHospital ? "HOSPITAL" : "VITIMA",
            OriginLabel = goToHospital ? "Local da vítima" : dispatch.Unit.Name,
            DestinationLabel = goToHospital ? dispatch.Occurrence.SelectedHospitalName! : "Local da vítima",
            DistanceKm = Math.Round(route.DistanceKm, 1),
            EstimatedMinutes = Math.Round(route.DurationMinutes, 1),
            Origin = Point(origin, goToHospital ? "Local da vítima" : dispatch.Unit.Name),
            Destination = Point(destination, goToHospital ? dispatch.Occurrence.SelectedHospitalName! : "Local da vítima"),
            Geometry = route.Geometry.Select(point => new MapCoordinateDto
            {
                Latitude = point.Latitude,
                Longitude = point.Longitude
            }).ToList()
        };
    }

    private static MapPointDto Point(MapCoordinate point, string label) => new()
    {
        Latitude = point.Latitude,
        Longitude = point.Longitude,
        Label = label
    };
}
