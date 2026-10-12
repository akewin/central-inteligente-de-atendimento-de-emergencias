using Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Api.Tests.Units;

public sealed class EmergencyServiceMigrationTests
{
    [Fact]
    public void MigrationModel_MatchesCurrentModel_AndGeneratesUpgradeSql()
    {
        using var context = new ApiDbContext(new DbContextOptionsBuilder<ApiDbContext>()
            .UseNpgsql("Host=localhost;Database=model_validation;Username=test;Password=test").Options);

        Assert.False(context.Database.HasPendingModelChanges());
        var script = context.GetService<IMigrator>().GenerateScript(
            "20260902155027_EnforceExclusiveOperationalAssignment",
            "20261012120000_RenameEmergencyServices");
        Assert.Contains("UPDATE emergency_services SET type = 'GUARDA_CIVIL'", script);
        Assert.Contains("UPDATE emergency_services SET type = 'DEFESA_CIVIL'", script);
        Assert.True(script.IndexOf("DROP CONSTRAINT", StringComparison.Ordinal)
            < script.IndexOf("UPDATE emergency_services", StringComparison.Ordinal));
        Assert.True(script.IndexOf("UPDATE emergency_services", StringComparison.Ordinal)
            < script.IndexOf("ADD CONSTRAINT", StringComparison.Ordinal));
    }
}
