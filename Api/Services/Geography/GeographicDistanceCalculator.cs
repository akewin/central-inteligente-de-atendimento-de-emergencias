namespace Api.Services.Geography;

public sealed class GeographicDistanceCalculator
{
    private const double MeanEarthRadiusKm = 6371.0088d;

    public double CalculateKm(decimal latitude1, decimal longitude1, decimal latitude2, decimal longitude2)
    {
        ValidateCoordinates(latitude1, longitude1);
        ValidateCoordinates(latitude2, longitude2);

        var lat1 = ToRadians((double)latitude1);
        var lat2 = ToRadians((double)latitude2);
        var deltaLatitude = ToRadians((double)(latitude2 - latitude1));
        var deltaLongitude = ToRadians((double)(longitude2 - longitude1));

        var sinLatitude = Math.Sin(deltaLatitude / 2d);
        var sinLongitude = Math.Sin(deltaLongitude / 2d);
        var a = sinLatitude * sinLatitude
            + Math.Cos(lat1) * Math.Cos(lat2) * sinLongitude * sinLongitude;
        a = Math.Clamp(a, 0d, 1d);

        var centralAngle = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return MeanEarthRadiusKm * centralAngle;
    }

    public static bool AreValidCoordinates(decimal latitude, decimal longitude)
        => latitude is >= -90m and <= 90m && longitude is >= -180m and <= 180m;

    public static bool AreRoutableCoordinates(decimal latitude, decimal longitude)
        => latitude is > -85.05112878m and < 85.05112878m
            && longitude is > -180m and < 180m;

    private static void ValidateCoordinates(decimal latitude, decimal longitude)
    {
        if (!AreValidCoordinates(latitude, longitude))
            throw new ArgumentOutOfRangeException(nameof(latitude), "Coordenadas geográficas inválidas.");
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180d;
}
