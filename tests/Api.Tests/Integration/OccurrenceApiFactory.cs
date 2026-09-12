using Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Api.AI.Interfaces;
using Api.Services.Geography;
using Api.Geography;

namespace Api.Tests.Integration;

public sealed class OccurrenceApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;
    private readonly ILlmClient? _llmClient;

    public OccurrenceApiFactory(string connectionString, ILlmClient? llmClient = null)
    {
        _connectionString = connectionString;
        _llmClient = llmClient;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ApiDbContext>();
            services.RemoveAll<DbContextOptions<ApiDbContext>>();
            services.AddDbContext<ApiDbContext>(options =>
                options.UseNpgsql(_connectionString));
            services.RemoveAll<IGeocodingService>();
            services.AddSingleton<IGeocodingService, SimulatedGeocodingService>();
            if (_llmClient is not null)
            {
                services.RemoveAll<ILlmClient>();
                services.AddSingleton(_llmClient);
            }
        });
    }
}
