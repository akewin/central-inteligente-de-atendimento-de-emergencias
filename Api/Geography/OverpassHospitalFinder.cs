using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Api.Geography;

public sealed class OverpassHospitalFinder : IHospitalFinder
{
    private readonly HttpClient _httpClient;
    private readonly MapServicesOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OverpassHospitalFinder> _logger;

    public OverpassHospitalFinder(
        HttpClient httpClient,
        MapServicesOptions options,
        IMemoryCache cache,
        ILogger<OverpassHospitalFinder> logger)
        => (_httpClient, _options, _cache, _logger) = (httpClient, options, cache, logger);

    public async Task<IReadOnlyList<HospitalCandidate>> FindNearbyAsync(
        decimal latitude,
        decimal longitude,
        int radiusMeters,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = FormattableString.Invariant(
            $"nearby-hospitals:{Math.Round(latitude, 4)}:{Math.Round(longitude, 4)}:{radiusMeters}");
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<HospitalCandidate>? cached) && cached is not null)
            return cached;

        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);
        var query = $"""
            [out:json][timeout:15];
            (
              nwr(around:{radiusMeters},{lat},{lon})["amenity"="hospital"];
              nwr(around:{radiusMeters},{lat},{lon})["healthcare"="hospital"];
            );
            out center tags qt;
            """;

        Exception? lastError = null;
        foreach (var baseUrl in _options.OverpassBaseUrls)
        {
            try
            {
                var requestUri = $"{baseUrl.TrimEnd('/')}/interpreter?data={Uri.EscapeDataString(query)}";
                var payload = await _httpClient.GetFromJsonAsync<OverpassResponse>(requestUri, cancellationToken)
                    ?? throw new HttpRequestException("O serviço de hospitais retornou uma resposta vazia.");
                var result = MapCandidates(payload);
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
                return result;
            }
            catch (Exception error) when (IsEndpointFailure(error, cancellationToken))
            {
                lastError = error;
                _logger.LogWarning(error, "Instância Overpass indisponível: {OverpassHost}", new Uri(baseUrl).Host);
            }
        }

        throw new HttpRequestException(
            "Todas as instâncias configuradas para consulta de hospitais estão indisponíveis.",
            lastError);
    }

    private static IReadOnlyList<HospitalCandidate> MapCandidates(OverpassResponse payload)
        => payload.Elements
            .Select(MapCandidate)
            .Where(candidate => candidate is not null)
            .Cast<HospitalCandidate>()
            .GroupBy(candidate => candidate.Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToList();

    private static bool IsEndpointFailure(Exception error, CancellationToken cancellationToken)
        => error is HttpRequestException
            || error is JsonException
            || error is TaskCanceledException && !cancellationToken.IsCancellationRequested;

    private static HospitalCandidate? MapCandidate(OverpassElement element)
    {
        var latitude = element.Latitude ?? element.Center?.Latitude;
        var longitude = element.Longitude ?? element.Center?.Longitude;
        if (!latitude.HasValue || !longitude.HasValue)
            return null;

        var tags = element.Tags ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var name = FirstNonEmpty(tags, "name", "official_name", "short_name") ?? "Hospital sem nome informado";
        var address = BuildAddress(tags);
        var emergency = tags.TryGetValue("emergency", out var emergencyTag)
            && string.Equals(emergencyTag, "yes", StringComparison.OrdinalIgnoreCase);

        return new HospitalCandidate(
            $"{element.Type}/{element.Id}",
            name,
            address,
            latitude.Value,
            longitude.Value,
            emergency);
    }

    private static string BuildAddress(IReadOnlyDictionary<string, string> tags)
    {
        var street = FirstNonEmpty(tags, "addr:street", "addr:place");
        var number = FirstNonEmpty(tags, "addr:housenumber");
        var district = FirstNonEmpty(tags, "addr:suburb", "addr:district", "addr:neighbourhood");
        var city = FirstNonEmpty(tags, "addr:city", "addr:town", "addr:municipality");

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(street))
            parts.Add(string.IsNullOrWhiteSpace(number) ? street : $"{street}, {number}");
        if (!string.IsNullOrWhiteSpace(district)) parts.Add(district);
        if (!string.IsNullOrWhiteSpace(city)) parts.Add(city);
        return parts.Count == 0 ? "Endereço não informado no mapa" : string.Join(" — ", parts);
    }

    private static string? FirstNonEmpty(IReadOnlyDictionary<string, string> tags, params string[] keys)
    {
        foreach (var key in keys)
            if (tags.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        return null;
    }

    private sealed class OverpassResponse
    {
        [JsonPropertyName("elements")]
        public IReadOnlyList<OverpassElement> Elements { get; init; } = Array.Empty<OverpassElement>();
    }

    private sealed class OverpassElement
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("lat")]
        public decimal? Latitude { get; init; }

        [JsonPropertyName("lon")]
        public decimal? Longitude { get; init; }

        [JsonPropertyName("center")]
        public OverpassCenter? Center { get; init; }

        [JsonPropertyName("tags")]
        public IReadOnlyDictionary<string, string>? Tags { get; init; }
    }

    private sealed class OverpassCenter
    {
        [JsonPropertyName("lat")]
        public decimal Latitude { get; init; }

        [JsonPropertyName("lon")]
        public decimal Longitude { get; init; }
    }
}
