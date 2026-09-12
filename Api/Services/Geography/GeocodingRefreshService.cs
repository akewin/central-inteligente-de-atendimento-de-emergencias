using Api.Data;
using Api.Geography;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Geography;

/// <summary>Corrige, sob demanda, registros antigos criados pelo geocodificador simulado.</summary>
public sealed class GeocodingRefreshService
{
    private readonly ApiDbContext _context;
    private readonly IGeocodingService _geocoding;

    public GeocodingRefreshService(ApiDbContext context, IGeocodingService geocoding)
        => (_context, _geocoding) = (context, geocoding);

    public async Task RefreshAsync(Occurrence occurrence, CancellationToken cancellationToken)
    {
        if (!NeedsRefresh(occurrence.GeocodingSource) || !HasAddress(
                occurrence.PostalCode, occurrence.Street, occurrence.Number,
                occurrence.Neighborhood, occurrence.City, occurrence.State)) return;

        var result = await _geocoding.GeocodeAsync(new GeocodingAddress(
            occurrence.PostalCode!, occurrence.Street!, occurrence.Number!, occurrence.Complement,
            occurrence.Neighborhood!, occurrence.City!, occurrence.State!), cancellationToken);
        occurrence.Latitude = result.Latitude;
        occurrence.Longitude = result.Longitude;
        occurrence.GeocodingSource = result.Source;
        await _context.Occurrences.Where(item => item.Id == occurrence.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(item => item.Latitude, result.Latitude)
            .SetProperty(item => item.Longitude, result.Longitude)
            .SetProperty(item => item.GeocodingSource, result.Source), cancellationToken);
    }

    public async Task RefreshAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (!NeedsRefresh(unit.GeocodingSource) || !HasAddress(
                unit.PostalCode, unit.Street, unit.Number, unit.Neighborhood, unit.City, unit.State)) return;

        var result = await _geocoding.GeocodeAsync(new GeocodingAddress(
            unit.PostalCode!, unit.Street!, unit.Number!, unit.Complement,
            unit.Neighborhood!, unit.City!, unit.State!), cancellationToken);
        unit.Latitude = result.Latitude;
        unit.Longitude = result.Longitude;
        unit.GeocodingSource = result.Source;
        await _context.Units.Where(item => item.Id == unit.Id).ExecuteUpdateAsync(setters => setters
            .SetProperty(item => item.Latitude, result.Latitude)
            .SetProperty(item => item.Longitude, result.Longitude)
            .SetProperty(item => item.GeocodingSource, result.Source), cancellationToken);
    }

    private static bool NeedsRefresh(string? source)
        => string.IsNullOrWhiteSpace(source)
            || string.Equals(source, "SIMULATED", StringComparison.OrdinalIgnoreCase);

    private static bool HasAddress(params string?[] fields)
        => fields.All(value => !string.IsNullOrWhiteSpace(value));
}
