using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Models.Enums;
using Api.Security.Jwt;
using Api.Security.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Api.Tests.AI;
using Api.AI.Contracts;

namespace Api.Tests.Integration;

public sealed class OccurrencesApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:14-alpine")
        .WithDatabase("occurrence_api_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private OccurrenceApiFactory _factory = null!;
    private HttpClient _client = null!;
    private User _authorizedUser = null!;
    private User _forbiddenUser = null!;
    private readonly FakeLlmClient _llmClient = new();

    public async Task InitializeAsync()
    {
        ConfigureTestEnvironment();
        await _postgres.StartAsync();
        _factory = new OccurrenceApiFactory(_postgres.GetConnectionString(), _llmClient);
        _client = _factory.CreateClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var occurrenceResource = await context.SystemResources
            .SingleAsync(resource => resource.Name == "occurrences");

        _authorizedUser = CreateUser("authorized");
        _forbiddenUser = CreateUser("forbidden");
        context.Users.AddRange(_authorizedUser, _forbiddenUser);
        await context.SaveChangesAsync();

        context.AccessPermissions.Add(new AccessPermission
        {
            UserId = _authorizedUser.Id,
            SystemResourceId = occurrenceResource.Id
        });
        await context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task GetOccurrences_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await _client.GetAsync("/api/occurrences");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetOccurrences_WithoutPermission_ShouldReturnForbidden()
    {
        Authorize(_forbiddenUser, permissionId: 2);

        var response = await _client.GetAsync("/api/occurrences");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateOccurrence_ShouldUseAuthenticatedUserAndWriteSafeAudit()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var marker = $"academic-{Guid.NewGuid():N}";
        var payload = new
        {
            description = $"Acidente simulado {marker}",
            postalCode = "01001-000",
            street = "Praça da Sé",
            number = "100",
            neighborhood = "Sé",
            city = "São Paulo",
            state = "SP",
            reference = "Próximo ao marco zero",
            latitude = 90,
            longitude = 180,
            createdByUserId = int.MaxValue,
            status = "FINALIZADA"
        };

        var response = await _client.PostAsJsonAsync("/api/occurrences", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<OccurrenceReadDto>();
        Assert.NotNull(created);
        Assert.Equal(_authorizedUser.Id, created.CreatedBy.Id);
        Assert.Equal("ABERTA", created.Status);
        Assert.Null(created.ConfirmedType);
        Assert.Null(created.ConfirmedPriority);
        Assert.Equal("01001000", created.PostalCode);
        Assert.Equal("Praça da Sé", created.Street);
        Assert.Contains("Praça da Sé, 100", created.LocationDescription);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var persistedOccurrence = await context.Occurrences.AsNoTracking().SingleAsync(item => item.Id == created.Id);
        Assert.NotEqual(90m, persistedOccurrence.Latitude);
        Assert.NotEqual(180m, persistedOccurrence.Longitude);
        Assert.Equal("SIMULATED", persistedOccurrence.GeocodingSource);
        var log = await context.SystemLogs
            .AsNoTracking()
            .SingleAsync(item => item.Action == $"create Occurrence id: {created.Id}");

        Assert.Equal(_authorizedUser.Id, log.UserId);
        Assert.DoesNotContain(marker, log.Data ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("latitude", log.Data ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("longitude", log.Data ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("location", log.Data ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("", "01001000", "Rua válida", "SP")]
    [InlineData("Descrição válida", "123", "Rua válida", "SP")]
    [InlineData("Descrição válida", "01001000", "", "SP")]
    [InlineData("Descrição válida", "01001000", "Rua válida", "S1")]
    public async Task CreateOccurrence_WithInvalidPayload_ShouldReturnBadRequest(
        string description,
        string postalCode,
        string street,
        string state)
    {
        Authorize(_authorizedUser, permissionId: 5);
        var response = await _client.PostAsJsonAsync("/api/occurrences", new
        {
            description,
            postalCode,
            street,
            number = "10",
            neighborhood = "Centro",
            city = "São Paulo",
            state
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("error", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ListAndGetOccurrence_ShouldUsePersistencePaginationAndFilters()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var marker = $"filter-{Guid.NewGuid():N}";
        var first = await CreateAsync($"{marker} primeira");
        await CreateAsync($"{marker} segunda");

        var listResponse = await _client.GetAsync(
            $"/api/occurrences?page=1&pageSize=1&status=ABERTA&search={marker}");

        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        using var listJson = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync());
        Assert.Equal(2, listJson.RootElement.GetProperty("totalItems").GetInt32());
        Assert.Single(listJson.RootElement.GetProperty("data").EnumerateArray());

        var detailsResponse = await _client.GetAsync($"/api/occurrences/{first.Id}");
        Assert.Equal(HttpStatusCode.OK, detailsResponse.StatusCode);
        var details = await detailsResponse.Content.ReadFromJsonAsync<OccurrenceReadDto>();
        Assert.Equal(first.Id, details?.Id);
        Assert.Contains(marker, details?.Description);
    }

    [Theory]
    [InlineData("/api/occurrences?page=0")]
    [InlineData("/api/occurrences?pageSize=101")]
    [InlineData("/api/occurrences?status=UNKNOWN")]
    public async Task ListOccurrences_WithInvalidFilters_ShouldReturnBadRequest(string path)
    {
        Authorize(_authorizedUser, permissionId: 5);

        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOccurrence_WhenMissing_ShouldReturnNotFound()
    {
        Authorize(_authorizedUser, permissionId: 5);

        var response = await _client.GetAsync("/api/occurrences/2147483647");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RequestAnalysis_WithValidOutput_PersistsHistoryUpdatesStatusAndWritesSafeAudit()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var marker = $"sensitive-{Guid.NewGuid():N}";
        var occurrence = await CreateAsync($"Acidente acadêmico {marker}");

        var response = await _client.PostAsync($"/api/occurrences/{occurrence.Id}/analysis", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var analysis = await response.Content.ReadFromJsonAsync<AIAnalysisReadDto>();
        Assert.NotNull(analysis);
        Assert.Equal(new[] { "POLICIA", "SAMU" }, analysis.RecommendedServices);
        Assert.Contains(marker, _llmClient.LastRequest?.Content);
        Assert.DoesNotContain("latitude", _llmClient.LastRequest?.Content ?? "", StringComparison.OrdinalIgnoreCase);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var persistedOccurrence = await context.Occurrences.AsNoTracking()
            .SingleAsync(o => o.Id == occurrence.Id);
        Assert.Equal("AGUARDANDO_CONFIRMACAO", persistedOccurrence.Status.ToString());
        Assert.Null(persistedOccurrence.ConfirmedType);
        Assert.Null(persistedOccurrence.ConfirmedPriority);
        Assert.Single(await context.AIAnalyses.Where(a => a.OccurrenceId == occurrence.Id).ToListAsync());

        var audit = await context.SystemLogs.AsNoTracking()
            .SingleAsync(log => log.Action == $"create AIAnalysis id: {analysis.Id}");
        Assert.DoesNotContain(marker, audit.Data ?? "", StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("reason", audit.Data ?? "", StringComparison.OrdinalIgnoreCase);

        var details = await _client.GetFromJsonAsync<OccurrenceReadDto>($"/api/occurrences/{occurrence.Id}");
        Assert.Equal(analysis.Id, details?.LatestAIAnalysis?.Id);
    }

    [Fact]
    public async Task RequestAnalysis_WithInvalidOutput_DoesNotPersistOrChangeStatus()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var occurrence = await CreateAsync("Ocorrência com retorno inválido");
        _llmClient.Response = new LlmResponse("not-json", "FakeProvider", "fake-model");

        var response = await _client.PostAsync($"/api/occurrences/{occurrence.Id}/analysis", null);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.False(await context.AIAnalyses.AnyAsync(a => a.OccurrenceId == occurrence.Id));
        Assert.Equal(OccurrenceStatus.ABERTA,
            (await context.Occurrences.AsNoTracking().SingleAsync(o => o.Id == occurrence.Id)).Status);
    }

    [Fact]
    public async Task RequestAnalysis_WhenProviderTimesOut_DoesNotPersistOrChangeStatus()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var occurrence = await CreateAsync("Ocorrência com timeout simulado");
        _llmClient.Exception = new OperationCanceledException("timeout simulado");

        var response = await _client.PostAsync($"/api/occurrences/{occurrence.Id}/analysis", null);

        Assert.Equal(HttpStatusCode.GatewayTimeout, response.StatusCode);
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.False(await context.AIAnalyses.AnyAsync(a => a.OccurrenceId == occurrence.Id));
        Assert.Equal(OccurrenceStatus.ABERTA,
            (await context.Occurrences.AsNoTracking().SingleAsync(o => o.Id == occurrence.Id)).Status);
    }

    [Fact]
    public async Task RequestAnalysis_WithoutPermission_IsForbiddenAndDoesNotCallProvider()
    {
        Authorize(_forbiddenUser, permissionId: 2);
        var response = await _client.PostAsync("/api/occurrences/1/analysis", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null(_llmClient.LastRequest);
    }

    [Fact]
    public async Task ConfirmServices_CanAdjustRecommendationAndPreservesAnalysisAndHumanAuthorship()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var occurrence = await CreateAsync("Confirmação humana ajustada");
        var analysis = await RequestAnalysisAsync(occurrence.Id);

        var response = await _client.PutAsJsonAsync(
            $"/api/occurrences/{occurrence.Id}/service-confirmation",
            new { confirmedServices = new[] { "BOMBEIROS" } });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var confirmation = await response.Content
            .ReadFromJsonAsync<OccurrenceServiceConfirmationReadDto>();
        Assert.NotNull(confirmation);
        Assert.Equal(new[] { "BOMBEIROS" }, confirmation.ConfirmedServices);
        Assert.Equal(_authorizedUser.Id, confirmation.ConfirmedBy.Id);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var persistedAnalysis = await context.AIAnalyses.AsNoTracking()
            .SingleAsync(a => a.Id == analysis.Id);
        Assert.True(persistedAnalysis.RecommendsPolice);
        Assert.True(persistedAnalysis.RecommendsSamu);
        Assert.False(persistedAnalysis.RecommendsFireDepartment);

        var persisted = await context.Occurrences.AsNoTracking()
            .SingleAsync(o => o.Id == occurrence.Id);
        Assert.Equal(OccurrenceStatus.AGUARDANDO_CONFIRMACAO, persisted.Status);
        Assert.False(persisted.PoliceConfirmed);
        Assert.False(persisted.SamuConfirmed);
        Assert.True(persisted.FireDepartmentConfirmed);
        Assert.Equal(_authorizedUser.Id, persisted.ServicesConfirmedByUserId);
        Assert.NotNull(persisted.ServicesConfirmedAt);
        Assert.Null(persisted.ConfirmedType);
        Assert.Null(persisted.ConfirmedPriority);
        Assert.Contains(context.Model.GetEntityTypes(),
            entity => entity.ClrType == typeof(Dispatch));
        Assert.False(await context.Dispatches.AnyAsync(item => item.OccurrenceId == occurrence.Id));

        var details = await _client.GetFromJsonAsync<OccurrenceReadDto>(
            $"/api/occurrences/{occurrence.Id}");
        Assert.Equal(new[] { "POLICIA", "SAMU" },
            details?.LatestAIAnalysis?.RecommendedServices);
        Assert.Equal(new[] { "BOMBEIROS" },
            details?.ServiceConfirmation?.ConfirmedServices);
    }

    [Fact]
    public async Task ConfirmServices_RepeatingSameDecisionIsIdempotent_ButDifferentDecisionConflicts()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var occurrence = await CreateAsync("Confirmação idempotente");
        await RequestAnalysisAsync(occurrence.Id);
        var payload = new { confirmedServices = new[] { "POLICIA", "SAMU" } };

        var first = await _client.PutAsJsonAsync(
            $"/api/occurrences/{occurrence.Id}/service-confirmation", payload);
        var firstBody = await first.Content.ReadFromJsonAsync<OccurrenceServiceConfirmationReadDto>();
        var repeated = await _client.PutAsJsonAsync(
            $"/api/occurrences/{occurrence.Id}/service-confirmation", payload);
        var repeatedBody = await repeated.Content.ReadFromJsonAsync<OccurrenceServiceConfirmationReadDto>();
        var conflict = await _client.PutAsJsonAsync(
            $"/api/occurrences/{occurrence.Id}/service-confirmation",
            new { confirmedServices = new[] { "BOMBEIROS" } });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        Assert.Equal(firstBody?.ConfirmedAt, repeatedBody?.ConfirmedAt);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.Single(await context.SystemLogs.Where(log =>
            log.Action == $"update OccurrenceServiceConfirmation id: {occurrence.Id}").ToListAsync());
    }

    [Theory]
    [InlineData()]
    [InlineData("SAMU", "SAMU")]
    [InlineData("HOSPITAL")]
    public async Task ConfirmServices_WithInvalidSelection_ReturnsBadRequest(params string[] services)
    {
        Authorize(_authorizedUser, permissionId: 5);
        var response = await _client.PutAsJsonAsync(
            "/api/occurrences/1/service-confirmation",
            new { confirmedServices = services });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmServices_InOpenStatusOrWithoutPermission_IsRejected()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var occurrence = await CreateAsync("Ainda sem análise");
        var wrongStatus = await _client.PutAsJsonAsync(
            $"/api/occurrences/{occurrence.Id}/service-confirmation",
            new { confirmedServices = new[] { "SAMU" } });
        Assert.Equal(HttpStatusCode.Conflict, wrongStatus.StatusCode);

        Authorize(_forbiddenUser, permissionId: 2);
        var forbidden = await _client.PutAsJsonAsync(
            $"/api/occurrences/{occurrence.Id}/service-confirmation",
            new { confirmedServices = new[] { "SAMU" } });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    [Fact]
    public async Task RecommendUnits_FiltersAndOrdersWithoutMutatingDatabase()
    {
        Authorize(_authorizedUser, permissionId: BasePermissions.OCCURRENCES);
        int occurrenceId;
        int availablePoliceId;
        int unitCountBefore;
        int logCountBefore;

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var police = await context.EmergencyServices.SingleAsync(service => service.Type == EmergencyServiceType.POLICIA);
            var samu = await context.EmergencyServices.SingleAsync(service => service.Type == EmergencyServiceType.SAMU);
            var occurrence = new Occurrence
            {
                Description = "Ocorrência para recomendação de equipes",
                LocationDescription = "Ponto acadêmico",
                Latitude = 10m,
                Longitude = 10m,
                Status = OccurrenceStatus.AGUARDANDO_CONFIRMACAO,
                PoliceConfirmed = true,
                ServicesConfirmedAt = DateTime.UtcNow,
                ServicesConfirmedByUserId = _authorizedUser.Id,
                CreatedByUserId = _authorizedUser.Id
            };
            var availablePolice = new Unit
            {
                Name = $"PM-NEAR-{Guid.NewGuid():N}", NormalizedName = $"PM-NEAR-{Guid.NewGuid():N}",
                EmergencyServiceId = police.Id, Latitude = 10m, Longitude = 10m, Status = UnitStatus.DISPONIVEL
            };
            var unavailablePolice = new Unit
            {
                Name = $"PM-BUSY-{Guid.NewGuid():N}", NormalizedName = $"PM-BUSY-{Guid.NewGuid():N}",
                EmergencyServiceId = police.Id, Latitude = 10m, Longitude = 10m, Status = UnitStatus.EM_ATENDIMENTO
            };
            var unconfirmedSamu = new Unit
            {
                Name = $"SAMU-UNCONFIRMED-{Guid.NewGuid():N}", NormalizedName = $"SAMU-UNCONFIRMED-{Guid.NewGuid():N}",
                EmergencyServiceId = samu.Id, Latitude = 10m, Longitude = 10m, Status = UnitStatus.DISPONIVEL
            };
            context.AddRange(occurrence, availablePolice, unavailablePolice, unconfirmedSamu);
            await context.SaveChangesAsync();
            occurrenceId = occurrence.Id;
            availablePoliceId = availablePolice.Id;
            unitCountBefore = await context.Units.CountAsync();
            logCountBefore = await context.SystemLogs.CountAsync();
        }

        var response = await _client.GetAsync($"/api/occurrences/{occurrenceId}/unit-recommendations?limitPerService=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var recommendations = await response.Content.ReadFromJsonAsync<OccurrenceUnitRecommendationsDto>();
        var group = Assert.Single(recommendations!.Groups);
        Assert.Equal("POLICIA", group.Service);
        Assert.Equal(availablePoliceId, group.Units.First().Id);
        Assert.Equal(0d, group.Units.First().DistanceKm);
        Assert.All(group.Units, unit => Assert.Equal("DISPONIVEL", unit.Status));
        Assert.DoesNotContain(group.Units, unit => unit.Name.Contains("BUSY"));

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.Equal(unitCountBefore, await verificationContext.Units.CountAsync());
        Assert.Equal(logCountBefore, await verificationContext.SystemLogs.CountAsync());
        Assert.Equal(OccurrenceStatus.AGUARDANDO_CONFIRMACAO,
            await verificationContext.Occurrences.Where(item => item.Id == occurrenceId).Select(item => item.Status).SingleAsync());
    }

    private async Task<OccurrenceReadDto> CreateAsync(string description)
    {
        var response = await _client.PostAsJsonAsync("/api/occurrences", new
        {
            description,
            postalCode = "01001000",
            street = "Praça da Sé",
            number = "100",
            neighborhood = "Sé",
            city = "São Paulo",
            state = "SP",
            reference = "Laboratório de Integração"
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OccurrenceReadDto>())!;
    }

    [Fact]
    public async Task ConfirmDispatch_ShouldPersistAtomicallyAndRejectDuplicateRequest()
    {
        Authorize(_authorizedUser, permissionId: 5);
        int occurrenceId;
        int policeUnitId;
        int samuUnitId;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var police = await context.EmergencyServices.SingleAsync(item => item.Type == EmergencyServiceType.POLICIA);
            var samu = await context.EmergencyServices.SingleAsync(item => item.Type == EmergencyServiceType.SAMU);
            var occurrence = CreateDispatchReadyOccurrence(police: true, samu: true);
            var policeUnit = CreateAvailableUnit(police.Id, "PM-DISPATCH");
            var samuUnit = CreateAvailableUnit(samu.Id, "SAMU-DISPATCH");
            context.AddRange(occurrence, policeUnit, samuUnit);
            await context.SaveChangesAsync();
            occurrenceId = occurrence.Id;
            policeUnitId = policeUnit.Id;
            samuUnitId = samuUnit.Id;
        }

        var response = await _client.PostAsJsonAsync($"/api/occurrences/{occurrenceId}/dispatches", new
        {
            unitIds = new[] { samuUnitId, policeUnitId }
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<DispatchConfirmationReadDto>();
        Assert.Equal("DESPACHADA", result!.OccurrenceStatus);
        Assert.Equal(_authorizedUser.Id, result.ConfirmedBy.Id);
        Assert.Equal(2, result.Dispatches.Count);
        Assert.All(result.Dispatches, item => Assert.Equal("RESERVADA", item.UnitStatus));
        Assert.All(result.Dispatches, item => Assert.Equal("ATRIBUIDO", item.Status));

        var duplicate = await _client.PostAsJsonAsync($"/api/occurrences/{occurrenceId}/dispatches", new
        {
            unitIds = new[] { policeUnitId, samuUnitId }
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.Equal(2, await verificationContext.Dispatches.CountAsync(item => item.OccurrenceId == occurrenceId));
        Assert.Equal(OccurrenceStatus.DESPACHADA,
            await verificationContext.Occurrences.Where(item => item.Id == occurrenceId).Select(item => item.Status).SingleAsync());
        Assert.All(await verificationContext.Units.Where(item => item.Id == policeUnitId || item.Id == samuUnitId).ToListAsync(),
            unit => Assert.Equal(UnitStatus.RESERVADA, unit.Status));
        Assert.True(await verificationContext.SystemLogs.AnyAsync(log =>
            log.Action == $"confirm Dispatch occurrence id: {occurrenceId}" && log.UserId == _authorizedUser.Id));
    }

    [Fact]
    public async Task ConfirmDispatch_WhenAnyUnitIsUnavailable_ShouldRollbackEntireOperation()
    {
        Authorize(_authorizedUser, permissionId: 5);
        int occurrenceId;
        int availableUnitId;
        int unavailableUnitId;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var police = await context.EmergencyServices.SingleAsync(item => item.Type == EmergencyServiceType.POLICIA);
            var samu = await context.EmergencyServices.SingleAsync(item => item.Type == EmergencyServiceType.SAMU);
            var occurrence = CreateDispatchReadyOccurrence(police: true, samu: true);
            var available = CreateAvailableUnit(police.Id, "PM-ROLLBACK");
            var unavailable = CreateAvailableUnit(samu.Id, "SAMU-ROLLBACK");
            unavailable.Status = UnitStatus.EM_ATENDIMENTO;
            context.AddRange(occurrence, available, unavailable);
            await context.SaveChangesAsync();
            occurrenceId = occurrence.Id;
            availableUnitId = available.Id;
            unavailableUnitId = unavailable.Id;
        }

        var response = await _client.PostAsJsonAsync($"/api/occurrences/{occurrenceId}/dispatches", new
        {
            unitIds = new[] { availableUnitId, unavailableUnitId }
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var contextAfter = verificationScope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.False(await contextAfter.Dispatches.AnyAsync(item => item.OccurrenceId == occurrenceId));
        Assert.Equal(OccurrenceStatus.AGUARDANDO_CONFIRMACAO,
            await contextAfter.Occurrences.Where(item => item.Id == occurrenceId).Select(item => item.Status).SingleAsync());
        Assert.Equal(UnitStatus.DISPONIVEL,
            await contextAfter.Units.Where(item => item.Id == availableUnitId).Select(item => item.Status).SingleAsync());
    }

    [Fact]
    public async Task ConfirmDispatch_ConcurrentRequestsForSameUnit_ShouldAllowOnlyOne()
    {
        Authorize(_authorizedUser, permissionId: 5);
        int firstOccurrenceId;
        int secondOccurrenceId;
        int unitId;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var police = await context.EmergencyServices.SingleAsync(item => item.Type == EmergencyServiceType.POLICIA);
            var first = CreateDispatchReadyOccurrence(police: true);
            var second = CreateDispatchReadyOccurrence(police: true);
            var unit = CreateAvailableUnit(police.Id, "PM-CONCURRENT");
            context.AddRange(first, second, unit);
            await context.SaveChangesAsync();
            firstOccurrenceId = first.Id;
            secondOccurrenceId = second.Id;
            unitId = unit.Id;
        }

        var firstRequest = _client.PostAsJsonAsync($"/api/occurrences/{firstOccurrenceId}/dispatches", new { unitIds = new[] { unitId } });
        var secondRequest = _client.PostAsJsonAsync($"/api/occurrences/{secondOccurrenceId}/dispatches", new { unitIds = new[] { unitId } });
        var responses = await Task.WhenAll(firstRequest, secondRequest);

        Assert.Single(responses, item => item.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, item => item.StatusCode == HttpStatusCode.Conflict);
        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var contextAfter = verificationScope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.Equal(1, await contextAfter.Dispatches.CountAsync(item => item.UnitId == unitId));
        Assert.Equal(1, await contextAfter.Occurrences.CountAsync(item =>
            (item.Id == firstOccurrenceId || item.Id == secondOccurrenceId) && item.Status == OccurrenceStatus.DESPACHADA));
    }

    private Occurrence CreateDispatchReadyOccurrence(bool police = false, bool samu = false, bool firefighters = false)
        => new()
        {
            Description = $"Ocorrência para despacho {Guid.NewGuid():N}",
            LocationDescription = "Local de teste",
            Latitude = -23.550520m,
            Longitude = -46.633308m,
            Status = OccurrenceStatus.AGUARDANDO_CONFIRMACAO,
            PoliceConfirmed = police,
            SamuConfirmed = samu,
            FireDepartmentConfirmed = firefighters,
            ServicesConfirmedByUserId = _authorizedUser.Id,
            ServicesConfirmedAt = DateTime.UtcNow,
            CreatedByUserId = _authorizedUser.Id
        };

    [Fact]
    public async Task TrackOccurrence_DispatchedToAttendanceToFinalized_ShouldSynchronizeUnitAndTimeline()
    {
        Authorize(_authorizedUser, permissionId: 5);
        int occurrenceId;
        int unitId;
        int dispatchId;
        DateTime dispatchUpdatedAt;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var police = await context.EmergencyServices.SingleAsync(item => item.Type == EmergencyServiceType.POLICIA);
            var occurrence = CreateDispatchReadyOccurrence(police: true);
            occurrence.Status = OccurrenceStatus.DESPACHADA;
            var unit = CreateAvailableUnit(police.Id, "PM-TRACK");
            unit.Status = UnitStatus.RESERVADA;
            var dispatch = new Dispatch { Occurrence = occurrence, Unit = unit, ConfirmedByUserId = _authorizedUser.Id, Status = DispatchStatus.ATRIBUIDO };
            context.AddRange(occurrence, unit, dispatch);
            await context.SaveChangesAsync();
            var operationalUser = await context.Users.SingleAsync(item => item.Id == _authorizedUser.Id);
            operationalUser.UnitId = unit.Id;
            context.AccessPermissions.Add(new AccessPermission { UserId = operationalUser.Id, SystemResourceId = BasePermissions.UNIT_OPERATIONS });
            await context.SaveChangesAsync();
            _authorizedUser.UnitId = unit.Id;
            occurrenceId = occurrence.Id;
            unitId = unit.Id;
            dispatchId = dispatch.Id;
            dispatchUpdatedAt = dispatch.UpdatedAt;
        }

        Authorize(_authorizedUser, BasePermissions.UNIT_OPERATIONS, unitId);
        foreach (var target in new[] { "ACEITO", "NO_LOCAL", "EM_ATENDIMENTO", "CONCLUIDO" })
        {
            var response = await _client.PostAsJsonAsync($"/api/unit-operations/dispatches/{dispatchId}/transitions", new
            { targetStatus = target, expectedUpdatedAt = dispatchUpdatedAt });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<OperationalDispatchReadDto>();
            dispatchUpdatedAt = result!.UpdatedAt;
        }

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var contextAfter = verificationScope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.Equal(OccurrenceStatus.FINALIZADA,
            await contextAfter.Occurrences.Where(item => item.Id == occurrenceId).Select(item => item.Status).SingleAsync());
        Assert.Equal(UnitStatus.DISPONIVEL,
            await contextAfter.Units.Where(item => item.Id == unitId).Select(item => item.Status).SingleAsync());
        Assert.Equal(4, await contextAfter.SystemLogs.CountAsync(log =>
            log.Action.Contains("DispatchStatus") && log.UserId == _authorizedUser.Id));
    }

    [Fact]
    public async Task CancelOccurrence_BeforeDispatch_ShouldNotChangeUnits_AndRequiresPermissionAndReason()
    {
        Authorize(_authorizedUser, permissionId: 5);
        var occurrence = await CreateAsync($"Cancelamento controlado {Guid.NewGuid():N}");

        var missingReason = await _client.PutAsJsonAsync($"/api/occurrences/{occurrence.Id}/status", new
        {
            targetStatus = "CANCELADA", expectedUpdatedAt = occurrence.UpdatedAt, reason = "não"
        });
        Assert.Equal(HttpStatusCode.BadRequest, missingReason.StatusCode);

        Authorize(_forbiddenUser, permissionId: 2);
        var forbidden = await _client.PutAsJsonAsync($"/api/occurrences/{occurrence.Id}/status", new
        {
            targetStatus = "CANCELADA", expectedUpdatedAt = occurrence.UpdatedAt, reason = "Registro duplicado"
        });
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        Authorize(_authorizedUser, permissionId: 5);
        var response = await _client.PutAsJsonAsync($"/api/occurrences/{occurrence.Id}/status", new
        {
            targetStatus = "CANCELADA", expectedUpdatedAt = occurrence.UpdatedAt, reason = "Registro duplicado"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        Assert.Equal(OccurrenceStatus.CANCELADA,
            await context.Occurrences.Where(item => item.Id == occurrence.Id).Select(item => item.Status).SingleAsync());
        Assert.False(await context.Dispatches.AnyAsync(item => item.OccurrenceId == occurrence.Id));
    }

    [Fact]
    public async Task TrackOccurrence_ConcurrentCommands_ShouldCommitOnlyOneConsistentTransition()
    {
        Authorize(_authorizedUser, permissionId: 5);
        int occurrenceId;
        int unitId;
        DateTime expectedUpdatedAt;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
            var police = await context.EmergencyServices.SingleAsync(item => item.Type == EmergencyServiceType.POLICIA);
            var occurrence = CreateDispatchReadyOccurrence(police: true);
            occurrence.Status = OccurrenceStatus.DESPACHADA;
            var unit = CreateAvailableUnit(police.Id, "PM-STATUS-RACE");
            unit.Status = UnitStatus.DESLOCAMENTO;
            context.AddRange(occurrence, unit, new Dispatch { Occurrence = occurrence, Unit = unit, ConfirmedByUserId = _authorizedUser.Id });
            await context.SaveChangesAsync();
            occurrenceId = occurrence.Id;
            unitId = unit.Id;
            expectedUpdatedAt = occurrence.UpdatedAt;
        }

        var attend = _client.PutAsJsonAsync($"/api/occurrences/{occurrenceId}/status",
            new { targetStatus = "EM_ATENDIMENTO", expectedUpdatedAt, reason = (string?)null });
        var cancel = _client.PutAsJsonAsync($"/api/occurrences/{occurrenceId}/status",
            new { targetStatus = "CANCELADA", expectedUpdatedAt, reason = "Cancelamento concorrente" });
        var responses = await Task.WhenAll(attend, cancel);
        Assert.Single(responses, item => item.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, item => item.StatusCode == HttpStatusCode.Conflict);

        await using var verificationScope = _factory.Services.CreateAsyncScope();
        var contextAfter = verificationScope.ServiceProvider.GetRequiredService<ApiDbContext>();
        var status = await contextAfter.Occurrences.Where(item => item.Id == occurrenceId).Select(item => item.Status).SingleAsync();
        var unitStatus = await contextAfter.Units.Where(item => item.Id == unitId).Select(item => item.Status).SingleAsync();
        Assert.Equal(OccurrenceStatus.CANCELADA, status);
        Assert.Equal(UnitStatus.DISPONIVEL, unitStatus);
    }

    private static Unit CreateAvailableUnit(int serviceId, string prefix)
    {
        var name = $"{prefix}-{Guid.NewGuid():N}";
        return new Unit
        {
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            EmergencyServiceId = serviceId,
            Latitude = -23.550520m,
            Longitude = -46.633308m,
            Status = UnitStatus.DISPONIVEL
        };
    }

    private async Task<AIAnalysisReadDto> RequestAnalysisAsync(int occurrenceId)
    {
        var response = await _client.PostAsync($"/api/occurrences/{occurrenceId}/analysis", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AIAnalysisReadDto>())!;
    }

    private void Authorize(User user, int permissionId, int? unitId = null)
    {
        var claims = new List<Claim>
        {
            new Claim("id", user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim("permission", permissionId.ToString())
        };
        if (unitId.HasValue) claims.Add(new Claim("unitId", unitId.Value.ToString()));
        var token = JwtServices.Create(claims);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static User CreateUser(string prefix)
    {
        var suffix = Guid.NewGuid().ToString("N");
        return new User
        {
            Username = $"{prefix}-{suffix}",
            Email = $"{prefix}-{suffix}@example.test",
            Password = "test-password-hash",
            FullName = $"{prefix} test user",
            Active = true
        };
    }

    private static void ConfigureTestEnvironment()
    {
        Environment.SetEnvironmentVariable("DB_HOST", "localhost");
        Environment.SetEnvironmentVariable("DB_PORT", "5432");
        Environment.SetEnvironmentVariable("DB_USER", "postgres");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "postgres");
        Environment.SetEnvironmentVariable("DB_NAME", "occurrence_api_tests");
        Environment.SetEnvironmentVariable("API_PORT", "5210");
        Environment.SetEnvironmentVariable(
            "JWT_SECRET_KEY",
            "integration-tests-only-secret-key-with-at-least-32-characters");
        Environment.SetEnvironmentVariable("WEB_APP_URL", "http://localhost:5173");
        Environment.SetEnvironmentVariable("RESEND_API_KEY", "unused-in-integration-tests");
        Environment.SetEnvironmentVariable("RESEND_FROM_EMAIL", "no-reply@example.invalid");
        Environment.SetEnvironmentVariable("RUN_USERS_SEED", "false");
    }
}
