using Api.Models;
using Api.Security.Permissions;

namespace Api.Tests.Hospitals;

public sealed class HospitalCapacityTests
{
    [Fact]
    public void AvailableBeds_SubtractsOccupiedAndReserved()
    {
        var ward = new HospitalWard { TotalBeds = 12, OccupiedBeds = 7, ReservedBeds = 2 };
        Assert.Equal(3, ward.AvailableBeds);
    }

    [Theory]
    [InlineData("GET", 9)]
    [InlineData("POST", 10)]
    public void HospitalAdministration_UsesDedicatedPermissions(string method, int permission)
        => Assert.Contains(permission, EndpointPermissions.GetRequiredPermissions("/api/hospitals", method));

    [Fact]
    public void HospitalReception_UsesOperationalPermission()
        => Assert.Equal([BasePermissions.HOSPITAL_OPERATIONS], EndpointPermissions.GetRequiredPermissions("/api/hospital-receptions", "GET"));
}
