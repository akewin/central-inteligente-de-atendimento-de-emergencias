using Api.Data;
using Api.Models;
using Api.Security.Passwords;
using Api.Security.Permissions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Api.Tests.Integration;

public sealed class HospitalPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:14-alpine")
        .WithDatabase("hospital_tests").WithUsername("postgres").WithPassword("postgres").Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }
    public Task DisposeAsync() => _postgres.DisposeAsync().AsTask();

    [Fact]
    public async Task HospitalSeed_IsIdempotentAndCreatesWards()
    {
        await using var context = CreateContext();
        await DbInitializer.SeedHospitalsAsync(context);
        await DbInitializer.SeedHospitalsAsync(context);
        Assert.Equal(5, await context.Hospitals.CountAsync());
        Assert.Equal(15, await context.HospitalWards.CountAsync());
    }

    [Fact]
    public async Task HospitalUserSeed_CreatesOneOperationalAccountPerHospitalAndIsIdempotent()
    {
        await using var context = CreateContext();
        await DbInitializer.SeedSystemResourcesAsync(context);
        await DbInitializer.SeedHospitalsAsync(context);

        await DbInitializer.SeedHospitalUsersAsync(context);
        await DbInitializer.SeedHospitalUsersAsync(context);

        var hospitalUsers = await context.Users
            .Include(user => user.AccessPermissions)
            .Where(user => user.HospitalId != null)
            .ToListAsync();

        Assert.Equal(5, hospitalUsers.Count);
        Assert.Equal(5, hospitalUsers.Select(user => user.HospitalId).Distinct().Count());
        Assert.All(hospitalUsers, user =>
        {
            Assert.True(user.Active);
            Assert.Null(user.UnitId);
            Assert.True(PasswordHash.Verify("Hospital@123", user.Password));
            Assert.Contains(user.AccessPermissions,
                permission => permission.SystemResourceId == BasePermissions.HOSPITAL_OPERATIONS);
        });
    }

    [Fact]
    public async Task Database_RejectsHospitalOutsideArarasAndOverbookedWard()
    {
        await using var context = CreateContext();
        var hospital = new Hospital { Name = "Fora", PostalCode = "00000000", Street = "Rua", Number = "1",
            Neighborhood = "Centro", City = "Limeira", State = "SP", Latitude = -22.5m, Longitude = -47.4m };
        context.Hospitals.Add(hospital);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        context.ChangeTracker.Clear();

        hospital = new Hospital { Name = "Araras", PostalCode = "13600000", Street = "Rua", Number = "1",
            Neighborhood = "Centro", City = "Araras", State = "SP", Latitude = -22.3m, Longitude = -47.3m };
        hospital.Wards.Add(new HospitalWard { Name = "Emergência", TotalBeds = 2, OccupiedBeds = 2, ReservedBeds = 1 });
        context.Hospitals.Add(hospital);
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private ApiDbContext CreateContext() => new(new DbContextOptionsBuilder<ApiDbContext>().UseNpgsql(_postgres.GetConnectionString()).Options);
}
