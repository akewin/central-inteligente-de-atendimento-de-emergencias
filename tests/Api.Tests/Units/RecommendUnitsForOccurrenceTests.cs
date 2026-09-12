using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Models;
using Api.Models.Enums;
using Api.Services.Geography;
using Api.Services.Units;

namespace Api.Tests.Units;

public sealed class RecommendUnitsForOccurrenceTests
{
    [Fact]
    public async Task Execute_FiltersUnavailableAndUnconfirmedServices_AndOrdersDeterministically()
    {
        var occurrence = ConfirmedOccurrence(police: true);
        var police = Service(1, EmergencyServiceType.POLICIA, "190");
        var samu = Service(2, EmergencyServiceType.SAMU, "192");
        var units = new List<Unit>
        {
            Unit(3, "PM-B", police, 0m, 0m),
            Unit(2, "PM-A", police, 0m, 0m),
            Unit(1, "PM-Z", police, 0m, 0m, UnitStatus.INDISPONIVEL),
            Unit(4, "SAMU-01", samu, 0m, 0m)
        };
        var service = CreateService(occurrence, [police], units);

        var result = await service.ExecuteAsync(occurrence.Id, 10);

        var group = Assert.Single(result.Groups);
        Assert.Equal("POLICIA", group.Service);
        Assert.Equal(new[] { "PM-A", "PM-B" }, group.Units.Select(unit => unit.Name));
        Assert.All(group.Units, unit => Assert.Equal("DISPONIVEL", unit.Status));
    }

    [Fact]
    public async Task Execute_AppliesLimitPerServiceAndReturnsEmptyConfirmedGroup()
    {
        var occurrence = ConfirmedOccurrence(police: true, samu: true);
        var police = Service(1, EmergencyServiceType.POLICIA, "190");
        var samu = Service(2, EmergencyServiceType.SAMU, "192");
        var service = CreateService(occurrence, [police, samu],
        [
            Unit(1, "PM-01", police, 0m, 0.01m),
            Unit(2, "PM-02", police, 0m, 0.02m)
        ]);

        var result = await service.ExecuteAsync(occurrence.Id, 1);

        Assert.Single(result.Groups.Single(group => group.Service == "POLICIA").Units);
        Assert.Empty(result.Groups.Single(group => group.Service == "SAMU").Units);
    }

    [Fact]
    public async Task Execute_DoesNotMutateOccurrenceOrUnits()
    {
        var occurrence = ConfirmedOccurrence(police: true);
        var police = Service(1, EmergencyServiceType.POLICIA, "190");
        var unit = Unit(1, "PM-01", police, 0m, 0m);
        var originalStatus = unit.Status;
        var originalOccurrenceUpdatedAt = occurrence.UpdatedAt;

        await CreateService(occurrence, [police], [unit]).ExecuteAsync(occurrence.Id);

        Assert.Equal(originalStatus, unit.Status);
        Assert.Equal(originalOccurrenceUpdatedAt, occurrence.UpdatedAt);
    }

    [Fact]
    public async Task Execute_WithoutConfirmation_IsConflict()
    {
        var occurrence = ConfirmedOccurrence();
        occurrence.ServicesConfirmedAt = null;
        var exception = await Assert.ThrowsAsync<AppException>(() =>
            CreateService(occurrence, [], []).ExecuteAsync(occurrence.Id));
        Assert.Equal(409, exception.StatusCode);
    }

    private static RecommendUnitsForOccurrence CreateService(
        Occurrence occurrence,
        List<EmergencyService> services,
        List<Unit> units)
        => new(new FakeOccurrenceRepository(occurrence), new FakeUnitRepository(services, units), new GeographicDistanceCalculator());

    private static Occurrence ConfirmedOccurrence(bool police = false, bool samu = false) => new()
    {
        Id = 10, Latitude = 0m, Longitude = 0m, Description = "Teste", LocationDescription = "Teste",
        Status = OccurrenceStatus.AGUARDANDO_CONFIRMACAO, PoliceConfirmed = police, SamuConfirmed = samu,
        ServicesConfirmedAt = DateTime.UtcNow, ServicesConfirmedByUserId = 1, UpdatedAt = DateTime.UtcNow
    };

    private static EmergencyService Service(int id, EmergencyServiceType type, string number) => new()
    { Id = id, Type = type, EmergencyNumber = number, DisplayName = type.ToString() };

    private static Unit Unit(int id, string name, EmergencyService service, decimal latitude, decimal longitude, UnitStatus status = UnitStatus.DISPONIVEL) => new()
    { Id = id, Name = name, NormalizedName = name, EmergencyService = service, EmergencyServiceId = service.Id, Latitude = latitude, Longitude = longitude, Status = status };

    private sealed class FakeOccurrenceRepository(Occurrence occurrence) : IOccurrenceRepository
    {
        public Task<Occurrence?> GetForUnitRecommendationAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<Occurrence?>(id == occurrence.Id ? occurrence : null);
        public Task<Occurrence> CreateAsync(Occurrence value) => throw new NotSupportedException();
        public Task<Occurrence?> GetByIdAsync(int id) => throw new NotSupportedException();
        public Task<Occurrence?> GetForAnalysisAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Occurrence?> GetForServiceConfirmationAsync(int id, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> CreatorExistsAsync(int userId) => throw new NotSupportedException();
        public IQueryable<Occurrence> Query() => throw new NotSupportedException();
    }

    private sealed class FakeUnitRepository(List<EmergencyService> services, List<Unit> units) : IUnitRepository
    {
        public Task<List<EmergencyService>> GetServicesAsync(IReadOnlyCollection<EmergencyServiceType> requested, CancellationToken cancellationToken = default)
            => Task.FromResult(services.Where(service => requested.Contains(service.Type)).ToList());
        public Task<List<Unit>> GetAvailableByServicesAsync(IReadOnlyCollection<EmergencyServiceType> requested, CancellationToken cancellationToken = default)
            => Task.FromResult(units);
        public Task<Unit> CreateAsync(Unit unit, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Unit?> GetByIdAsync(int id, bool tracked = false, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<EmergencyService?> GetServiceAsync(EmergencyServiceType type, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> NameExistsAsync(string normalizedName, int? exceptId = null, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<PagedResult<Unit>> GetPagedAsync(int page, int pageSize, string? search, EmergencyServiceType? service, UnitStatus? status, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
