using Api.Data;
using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Api.Tests.Integration;

public sealed class UnitPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:14-alpine")
        .WithDatabase("unit_tests").WithUsername("postgres").WithPassword("postgres").Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task SeedServicesAndUnits_IsIdempotent()
    {
        Environment.SetEnvironmentVariable("RUN_EMERGENCY_UNITS_SEED", "true");
        try
        {
            await using var context = CreateContext();
            await DbInitializer.SeedEmergencyServicesAsync(context);
            await DbInitializer.SeedEmergencyUnitsAsync(context);
            await DbInitializer.SeedEmergencyServicesAsync(context);
            await DbInitializer.SeedEmergencyUnitsAsync(context);

            Assert.Equal(3, await context.EmergencyServices.CountAsync());
            Assert.Equal(6, await context.Units.CountAsync());
        }
        finally { Environment.SetEnvironmentVariable("RUN_EMERGENCY_UNITS_SEED", null); }
    }

    [Fact]
    public async Task Database_RejectsDuplicateNormalizedNameAndInvalidCoordinates()
    {
        await using var context = CreateContext();
        await DbInitializer.SeedEmergencyServicesAsync(context);
        var serviceId = await context.EmergencyServices.Where(s => s.Type == EmergencyServiceType.POLICIA).Select(s => s.Id).SingleAsync();
        context.Units.Add(CreateUnit("PM-01", serviceId));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        context.Units.Add(CreateUnit("pm-01", serviceId));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        context.ChangeTracker.Clear();
        var invalid = CreateUnit("PM-02", serviceId);
        invalid.Latitude = 91;
        context.Units.Add(invalid);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private ApiDbContext CreateContext() => new(new DbContextOptionsBuilder<ApiDbContext>().UseNpgsql(_postgres.GetConnectionString()).Options);
    private static Unit CreateUnit(string name, int serviceId) => new()
    {
        Name = name.ToUpperInvariant(), NormalizedName = name.ToUpperInvariant(), EmergencyServiceId = serviceId,
        Latitude = -23.5m, Longitude = -46.6m, Status = UnitStatus.DISPONIVEL
    };
}
