using Api.Data;
using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
    public async Task RenameServices_PreservesExistingServiceAndUnitIds()
    {
        await using var context = CreateContext();
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20260902155027_EnforceExclusiveOperationalAssignment");
        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO emergency_services (id, type, emergency_number, display_name, created_at, updated_at)
            VALUES (101, 'POLICIA', '190', 'Polícia Militar', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                   (102, 'BOMBEIROS', '193', 'Corpo de Bombeiros', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            INSERT INTO units (id, name, normalized_name, emergency_service_id, latitude, longitude, status, created_at, updated_at)
            VALUES (101, 'PM-01', 'PM-01', 101, -23.5, -46.6, 'DISPONIVEL', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
                   (102, 'BM-01', 'BM-01', 102, -23.5, -46.6, 'DISPONIVEL', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);
            """);

        await migrator.MigrateAsync();
        await DbInitializer.SeedEmergencyServicesAsync(context);

        var guard = await context.EmergencyServices.SingleAsync(s => s.Id == 101);
        Assert.Equal(EmergencyServiceType.GUARDA_CIVIL, guard.Type);
        Assert.Equal("Guarda Civil", guard.DisplayName);
        Assert.Equal("153", guard.EmergencyNumber);
        var defense = await context.EmergencyServices.SingleAsync(s => s.Id == 102);
        Assert.Equal(EmergencyServiceType.DEFESA_CIVIL, defense.Type);
        Assert.Equal("Defesa Civil", defense.DisplayName);
        Assert.Equal("199", defense.EmergencyNumber);
        Assert.Equal(3, await context.EmergencyServices.CountAsync());
        var guardUnit = await context.Units.SingleAsync(u => u.Id == 101);
        Assert.Equal(101, guardUnit.EmergencyServiceId);
        Assert.Equal("GC-01", guardUnit.Name);
        var defenseUnit = await context.Units.SingleAsync(u => u.Id == 102);
        Assert.Equal(102, defenseUnit.EmergencyServiceId);
        Assert.Equal("DC-01", defenseUnit.Name);
    }

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
        var serviceId = await context.EmergencyServices.Where(s => s.Type == EmergencyServiceType.GUARDA_CIVIL).Select(s => s.Id).SingleAsync();
        context.Units.Add(CreateUnit("GC-01", serviceId));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        context.Units.Add(CreateUnit("gc-01", serviceId));
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        context.ChangeTracker.Clear();
        var invalid = CreateUnit("GC-02", serviceId);
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
