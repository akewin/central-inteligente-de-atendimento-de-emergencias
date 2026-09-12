using Api.Services.Geography;
using Api.Geography;

namespace Api.Tests.Units;

public sealed class SimulatedGeocodingServiceTests
{
    private readonly SimulatedGeocodingService _service = new();

    [Fact]
    public async Task GeocodeAsync_IsDeterministicAndReturnsValidInternalCoordinates()
    {
        var address = new GeocodingAddress("01001000", "Praça da Sé", "100", null, "Sé", "São Paulo", "SP");

        var first = await _service.GeocodeAsync(address);
        var second = await _service.GeocodeAsync(address);

        Assert.Equal(first, second);
        Assert.Equal("SIMULATED", first.Source);
        Assert.True(GeographicDistanceCalculator.AreValidCoordinates(first.Latitude, first.Longitude));
        Assert.InRange(first.Latitude, -23.590520m, -23.510520m);
        Assert.InRange(first.Longitude, -46.673308m, -46.593308m);
    }
}
