using Api.Security.Permissions;

namespace Api.Tests.Units;

public sealed class EndpointPermissionsTests
{
    [Fact]
    public void UnitsGet_AllowsOperationalOrManagementPermission()
        => Assert.Equal(
            new[] { BasePermissions.UNITS_VIEW, BasePermissions.UNITS_MANAGE },
            EndpointPermissions.GetRequiredPermissions("/api/units", "GET"));

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public void UnitsMutation_RequiresManagementPermission(string method)
        => Assert.Equal(
            new[] { BasePermissions.UNITS_MANAGE },
            EndpointPermissions.GetRequiredPermissions("/api/units/1", method));

    [Fact]
    public void ExistingRules_RemainCompatible()
        => Assert.Equal(
            new[] { BasePermissions.OCCURRENCES },
            EndpointPermissions.GetRequiredPermissions("/api/occurrences", "GET"));

    [Fact]
    public void OperationalRoutes_RequireDedicatedPermission()
        => Assert.Equal(
            new[] { BasePermissions.UNIT_OPERATIONS },
            EndpointPermissions.GetRequiredPermissions("/api/unit-operations/dispatches", "GET"));

    [Fact]
    public void RealtimeHub_AllowsOperationalAudiences()
        => Assert.Equal(
            new[] { BasePermissions.UNIT_OPERATIONS, BasePermissions.HOSPITAL_OPERATIONS, BasePermissions.OCCURRENCES },
            EndpointPermissions.GetRequiredPermissions("/hubs/dispatches", "GET"));
}
