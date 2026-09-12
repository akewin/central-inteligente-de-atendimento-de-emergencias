namespace Api.Geography;

public interface IGeocodingService
{
    Task<GeocodingResult> GeocodeAsync(GeocodingAddress address, CancellationToken cancellationToken = default);
}

public sealed record GeocodingAddress(
    string PostalCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State);

public sealed record GeocodingResult(decimal Latitude, decimal Longitude, string Source);
