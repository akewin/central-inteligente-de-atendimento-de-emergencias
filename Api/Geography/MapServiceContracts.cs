using Api.Dtos;

namespace Api.Geography;

public interface IHospitalFinder
{
    Task<IReadOnlyList<HospitalCandidate>> FindNearbyAsync(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        CancellationToken cancellationToken = default);
}

public interface IMapRoutingService
{
    Task<IReadOnlyList<RouteEstimate>> EstimateFromAsync(
        MapCoordinate origin,
        IReadOnlyList<MapCoordinate> destinations,
        CancellationToken cancellationToken = default);

    Task<MapRouteResult?> FindRouteAsync(
        IReadOnlyList<MapCoordinate> waypoints,
        CancellationToken cancellationToken = default);
}

public sealed record MapCoordinate(decimal Latitude, decimal Longitude);

public sealed record HospitalCandidate(
    string Id,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    bool HasEmergencyDepartment);

public sealed record RouteEstimate(double? DistanceKm, double? DurationMinutes);

public sealed record MapRouteLeg(double DistanceKm, double DurationMinutes);

public sealed record MapRouteResult(
    double DistanceKm,
    double DurationMinutes,
    IReadOnlyList<MapRouteLeg> Legs,
    IReadOnlyList<MapCoordinate> Geometry);
