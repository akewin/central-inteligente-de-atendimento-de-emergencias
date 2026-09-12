using Api.Services.Geography;

namespace Api.Tests.Units;

public sealed class GeographicDistanceCalculatorTests
{
    private readonly GeographicDistanceCalculator _calculator = new();

    [Fact]
    public void EqualCoordinates_ReturnZero()
        => Assert.Equal(0d, _calculator.CalculateKm(-23.550520m, -46.633308m, -23.550520m, -46.633308m), 9);

    [Fact]
    public void KnownPoints_ReturnExpectedDistanceAndAreSymmetric()
    {
        var parisToLondon = _calculator.CalculateKm(48.8566m, 2.3522m, 51.5074m, -0.1278m);
        var londonToParis = _calculator.CalculateKm(51.5074m, -0.1278m, 48.8566m, 2.3522m);

        Assert.InRange(parisToLondon, 343d, 344.5d);
        Assert.Equal(parisToLondon, londonToParis, 9);
    }

    [Fact]
    public void Antimeridian_UsesShortestArc()
        => Assert.InRange(_calculator.CalculateKm(0m, 179m, 0m, -179m), 222d, 223d);

    [Theory]
    [InlineData(90, 180, -90, -180)]
    [InlineData(-90, -180, 90, 180)]
    public void ValidCoordinateLimits_ReturnFiniteDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var distance = _calculator.CalculateKm(
            Convert.ToDecimal(lat1), Convert.ToDecimal(lon1), Convert.ToDecimal(lat2), Convert.ToDecimal(lon2));
        Assert.True(double.IsFinite(distance));
        Assert.True(distance >= 0);
    }

    [Theory]
    [InlineData(90.1, 0)]
    [InlineData(-90.1, 0)]
    [InlineData(0, 180.1)]
    [InlineData(0, -180.1)]
    public void InvalidCoordinates_AreRejected(double latitude, double longitude)
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.CalculateKm(Convert.ToDecimal(latitude), Convert.ToDecimal(longitude), 0m, 0m));

    [Theory]
    [InlineData(-22.348874, -47.337554, true)]
    [InlineData(90, 180, false)]
    [InlineData(80, 180, false)]
    [InlineData(85.05112878, 0, false)]
    public void RoutableCoordinates_RejectMapAndAntimeridianLimits(double latitude, double longitude, bool expected)
        => Assert.Equal(expected, GeographicDistanceCalculator.AreRoutableCoordinates(
            Convert.ToDecimal(latitude), Convert.ToDecimal(longitude)));
}
