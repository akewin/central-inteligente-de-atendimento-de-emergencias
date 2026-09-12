using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Api.Geography;

public sealed class OsrmMapRoutingService : IMapRoutingService
{
    private readonly HttpClient _httpClient;

    public OsrmMapRoutingService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<RouteEstimate>> EstimateFromAsync(
        MapCoordinate origin,
        IReadOnlyList<MapCoordinate> destinations,
        CancellationToken cancellationToken = default)
    {
        if (destinations.Count == 0)
            return Array.Empty<RouteEstimate>();

        var coordinates = string.Join(';', new[] { origin }.Concat(destinations).Select(FormatCoordinate));
        var destinationIndexes = string.Join(';', Enumerable.Range(1, destinations.Count));
        var url = $"table/v1/driving/{coordinates}?sources=0&destinations={destinationIndexes}&annotations=distance,duration";
        var payload = await _httpClient.GetFromJsonAsync<OsrmTableResponse>(url, cancellationToken)
            ?? throw new HttpRequestException("O roteador retornou uma resposta vazia.");
        EnsureOk(payload.Code, payload.Message);

        var distances = payload.Distances?.FirstOrDefault();
        var durations = payload.Durations?.FirstOrDefault();
        return Enumerable.Range(0, destinations.Count)
            .Select(index => new RouteEstimate(
                At(distances, index) is { } distance ? distance / 1000d : null,
                At(durations, index) is { } duration ? duration / 60d : null))
            .ToList();
    }

    public async Task<MapRouteResult?> FindRouteAsync(
        IReadOnlyList<MapCoordinate> waypoints,
        CancellationToken cancellationToken = default)
    {
        if (waypoints.Count < 2)
            throw new ArgumentException("A rota precisa de pelo menos dois pontos.", nameof(waypoints));

        var coordinates = string.Join(';', waypoints.Select(FormatCoordinate));
        var url = $"route/v1/driving/{coordinates}?alternatives=false&steps=false&geometries=geojson&overview=full";
        var payload = await _httpClient.GetFromJsonAsync<OsrmRouteResponse>(url, cancellationToken)
            ?? throw new HttpRequestException("O roteador retornou uma resposta vazia.");
        if (string.Equals(payload.Code, "NoRoute", StringComparison.OrdinalIgnoreCase))
            return null;
        EnsureOk(payload.Code, payload.Message);

        var route = payload.Routes.FirstOrDefault();
        if (route?.Geometry?.Coordinates is null)
            return null;

        return new MapRouteResult(
            route.Distance / 1000d,
            route.Duration / 60d,
            route.Legs.Select(leg => new MapRouteLeg(leg.Distance / 1000d, leg.Duration / 60d)).ToList(),
            route.Geometry.Coordinates
                .Where(coordinate => coordinate.Count >= 2)
                .Select(coordinate => new MapCoordinate(
                    Convert.ToDecimal(coordinate[1]),
                    Convert.ToDecimal(coordinate[0])))
                .ToList());
    }

    private static string FormatCoordinate(MapCoordinate coordinate)
        => $"{coordinate.Longitude.ToString(CultureInfo.InvariantCulture)},{coordinate.Latitude.ToString(CultureInfo.InvariantCulture)}";

    private static double? At(IReadOnlyList<double?>? values, int index)
        => values is not null && index < values.Count ? values[index] : null;

    private static void EnsureOk(string? code, string? message)
    {
        if (!string.Equals(code, "Ok", StringComparison.OrdinalIgnoreCase))
            throw new HttpRequestException(message ?? $"O roteador recusou a consulta ({code ?? "sem código"}).");
    }

    private sealed class OsrmTableResponse
    {
        [JsonPropertyName("code")]
        public string? Code { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("distances")]
        public IReadOnlyList<IReadOnlyList<double?>>? Distances { get; init; }

        [JsonPropertyName("durations")]
        public IReadOnlyList<IReadOnlyList<double?>>? Durations { get; init; }
    }

    private sealed class OsrmRouteResponse
    {
        [JsonPropertyName("code")]
        public string? Code { get; init; }

        [JsonPropertyName("message")]
        public string? Message { get; init; }

        [JsonPropertyName("routes")]
        public IReadOnlyList<OsrmRoute> Routes { get; init; } = Array.Empty<OsrmRoute>();
    }

    private sealed class OsrmRoute
    {
        [JsonPropertyName("distance")]
        public double Distance { get; init; }

        [JsonPropertyName("duration")]
        public double Duration { get; init; }

        [JsonPropertyName("legs")]
        public IReadOnlyList<OsrmLeg> Legs { get; init; } = Array.Empty<OsrmLeg>();

        [JsonPropertyName("geometry")]
        public OsrmGeometry? Geometry { get; init; }
    }

    private sealed class OsrmLeg
    {
        [JsonPropertyName("distance")]
        public double Distance { get; init; }

        [JsonPropertyName("duration")]
        public double Duration { get; init; }
    }

    private sealed class OsrmGeometry
    {
        [JsonPropertyName("coordinates")]
        public IReadOnlyList<IReadOnlyList<double>> Coordinates { get; init; } = Array.Empty<IReadOnlyList<double>>();
    }
}
