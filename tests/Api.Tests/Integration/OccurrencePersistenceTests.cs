using Api.Data;
using Api.Models;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Api.Tests.Integration;

public sealed class OccurrencePersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:14-alpine")
        .WithDatabase("occurrence_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task SaveOccurrence_ShouldPersistCreatorDefaultsAndEnums()
    {
        await using var context = CreateContext();
        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var occurrence = CreateOccurrence(user.Id);
        context.Occurrences.Add(occurrence);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var persisted = await context.Occurrences
            .AsNoTracking()
            .Include(o => o.CreatedByUser)
            .SingleAsync(o => o.Id == occurrence.Id);

        Assert.Equal(user.Id, persisted.CreatedByUserId);
        Assert.Equal(user.Username, persisted.CreatedByUser.Username);
        Assert.Equal(OccurrenceStatus.ABERTA, persisted.Status);
        Assert.Null(persisted.ConfirmedType);
        Assert.Null(persisted.ConfirmedPriority);
        Assert.NotEqual(default, persisted.CreatedAt);
        Assert.Equal(persisted.CreatedAt, persisted.UpdatedAt);
    }

    [Theory]
    [InlineData(90.000001, 0)]
    [InlineData(-90.000001, 0)]
    [InlineData(0, 180.000001)]
    [InlineData(0, -180.000001)]
    public async Task SaveOccurrence_ShouldRejectCoordinatesOutsideAllowedRange(
        double latitude,
        double longitude)
    {
        await using var context = CreateContext();
        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var occurrence = CreateOccurrence(user.Id);
        occurrence.Latitude = Convert.ToDecimal(latitude);
        occurrence.Longitude = Convert.ToDecimal(longitude);
        context.Occurrences.Add(occurrence);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveOccurrence_ShouldRejectUnknownCreator()
    {
        await using var context = CreateContext();
        context.Occurrences.Add(CreateOccurrence(int.MaxValue));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task DeleteCreator_ShouldBeRestrictedWhenOccurrenceExists()
    {
        await using var context = CreateContext();
        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.Occurrences.Add(CreateOccurrence(user.Id));
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var persistedCreator = await context.Users.SingleAsync(u => u.Id == user.Id);
        context.Users.Remove(persistedCreator);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveOccurrence_ShouldRejectUnknownEnumValue()
    {
        await using var context = CreateContext();
        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var occurrence = CreateOccurrence(user.Id);
        occurrence.Status = (OccurrenceStatus)999;
        context.Occurrences.Add(occurrence);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveOccurrence_ShouldRejectConfirmationWithoutUserAndTimestamp()
    {
        await using var context = CreateContext();
        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var occurrence = CreateOccurrence(user.Id);
        occurrence.SamuConfirmed = true;
        occurrence.ServicesConfirmedAt = DateTime.UtcNow;
        context.Occurrences.Add(occurrence);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private ApiDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApiDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        return new ApiDbContext(options);
    }

    private static User CreateUser()
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new User
        {
            Username = $"user-{suffix}",
            Email = $"{suffix}@example.test",
            Password = "test-password-hash",
            FullName = "Persistence Test User",
            Active = true
        };
    }

    private static Occurrence CreateOccurrence(int userId) => new()
    {
        Description = "Acidente de trânsito simulado com vítima consciente.",
        LocationDescription = "Avenida Acadêmica, 100",
        Latitude = -23.550520m,
        Longitude = -46.633308m,
        CreatedByUserId = userId
    };
}
