using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Api.Geography;
using Api.Middlewares;

namespace Api.Services.Geography;

public sealed class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;

    public NominatimGeocodingService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<GeocodingResult> GeocodeAsync(
        GeocodingAddress address,
        CancellationToken cancellationToken = default)
    {
        var result = await SearchAsync(BuildStructuredQuery(address), cancellationToken)
            ?? await SearchAsync(BuildFreeFormQuery(address), cancellationToken)
            ?? await SearchAsync(BuildPostalCodeQuery(address), cancellationToken);

        if (result is null
            || !decimal.TryParse(result.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
            || !decimal.TryParse(result.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude)
            || !GeographicDistanceCalculator.AreValidCoordinates(latitude, longitude))
        {
            throw new AppException("Não foi possível localizar esse endereço no mapa. Confira rua, número, cidade e CEP.", 422);
        }

        return new GeocodingResult(
            decimal.Round(latitude, 7),
            decimal.Round(longitude, 7),
            "NOMINATIM");
    }

    private async Task<NominatimResult?> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<List<NominatimResult>>(query, cancellationToken);
        return response?.FirstOrDefault();
    }

    private static string BuildStructuredQuery(GeocodingAddress address)
        => "search?format=jsonv2&limit=1&countrycodes=br"
            + $"&street={Encode($"{address.Number} {address.Street}")}" 
            + $"&city={Encode(address.City)}&state={Encode(address.State)}"
            + $"&postalcode={Encode(address.PostalCode)}";

    private static string BuildFreeFormQuery(GeocodingAddress address)
        => $"search?format=jsonv2&limit=1&countrycodes=br&q={Encode($"{address.Street}, {address.Number}, {address.Neighborhood}, {address.City}, {address.State}, {address.PostalCode}, Brasil")}";

    private static string BuildPostalCodeQuery(GeocodingAddress address)
        => "search?format=jsonv2&limit=1&countrycodes=br"
            + $"&postalcode={Encode(address.PostalCode)}";

    private static string Encode(string value) => Uri.EscapeDataString(value);

    private sealed class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string Latitude { get; init; } = string.Empty;

        [JsonPropertyName("lon")]
        public string Longitude { get; init; } = string.Empty;
    }
}
