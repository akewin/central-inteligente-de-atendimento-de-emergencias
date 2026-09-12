namespace Api.Geography;

public sealed class MapServicesOptions
{
    public const string DefaultOverpassBaseUrl = "https://overpass-api.de/api/";
    public const string DefaultOverpassFallbackBaseUrl = "https://overpass.private.coffee/api/";
    public const string DefaultOsrmBaseUrl = "https://router.project-osrm.org/";
    public const string DefaultNominatimBaseUrl = "https://nominatim.openstreetmap.org/";
    public const int DefaultTimeoutSeconds = 18;

    public IReadOnlyList<string> OverpassBaseUrls { get; init; } =
        [DefaultOverpassBaseUrl, DefaultOverpassFallbackBaseUrl];
    public string OsrmBaseUrl { get; init; } = DefaultOsrmBaseUrl;
    public string NominatimBaseUrl { get; init; } = DefaultNominatimBaseUrl;
    public int TimeoutSeconds { get; init; } = DefaultTimeoutSeconds;
    public string UserAgent { get; init; } = "FHO-Emergency-Academic-Prototype/1.0";
}
