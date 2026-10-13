using System.Security.Cryptography;
using System.Text;
using Api.Geography;
using Api.Services.Geography;

namespace Api.Tests.Support;

/// <summary>
/// Produz coordenadas estáveis dentro da área simulada de São Paulo.
/// Exclusivo dos testes: não realiza chamadas externas nem representa uma geocodificação real.
/// </summary>
public sealed class SimulatedGeocodingService : IGeocodingService
{
    public Task<GeocodingResult> GeocodeAsync(
        GeocodingAddress address,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalized = $"{address.PostalCode}|{address.Street}|{address.Number}|{address.City}|{address.State}".ToUpperInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        var latitudeOffset = BitConverter.ToUInt16(hash, 0) / 65535m * 0.08m - 0.04m;
        var longitudeOffset = BitConverter.ToUInt16(hash, 2) / 65535m * 0.08m - 0.04m;
        return Task.FromResult(new GeocodingResult(
            decimal.Round(-23.550520m + latitudeOffset, 6),
            decimal.Round(-46.633308m + longitudeOffset, 6),
            "SIMULATED"));
    }
}
