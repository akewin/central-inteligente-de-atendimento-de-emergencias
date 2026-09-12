using Api.Dtos;
using Api.Services.Maps;

namespace Api.Tests.Maps;

public sealed class HospitalRecommendationRankerTests
{
    [Fact]
    public void Rank_PrioritizesMappedEmergencyBeforeShorterEta()
    {
        var fasterWithoutEmergency = Hospital(1, "fast", emergency: false, minutes: 2.4, distanceKm: 1.3);
        var emergencyHospital = Hospital(2, "emergency", emergency: true, minutes: 6.5, distanceKm: 3.2);

        var ranked = HospitalRecommendationRanker.Rank(
            [fasterWithoutEmergency, emergencyHospital],
            limit: 8);

        Assert.Equal(2, ranked[0].Id);
        Assert.Equal(1, ranked[1].Id);
    }

    [Fact]
    public void Rank_UsesEtaWithinEmergencyGroup()
    {
        var slower = Hospital(1, "slower", emergency: true, minutes: 8, distanceKm: 4);
        var faster = Hospital(2, "faster", emergency: true, minutes: 5, distanceKm: 5);

        var ranked = HospitalRecommendationRanker.Rank([slower, faster], limit: 8);

        Assert.Equal(2, ranked[0].Id);
        Assert.Equal(1, ranked[1].Id);
    }

    private static MapHospitalDto Hospital(
        int id,
        string name,
        bool emergency,
        double minutes,
        double distanceKm)
        => new()
        {
            Id = id,
            Name = name,
            HasEmergencyDepartment = emergency,
            EstimatedMinutes = minutes,
            RoadDistanceKm = distanceKm,
            StraightLineDistanceKm = distanceKm
        };
}
