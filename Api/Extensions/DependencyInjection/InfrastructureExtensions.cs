using Api.AI.Interfaces;
using Api.AI.Providers;
using Api.Helpers;
using Api.Security.Jwt;
using Api.Security.Passwords;
using Api.Security.Policies;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using Api.Services.Geography;
using Api.Geography;
using Api.AI.Configuration;
using Api.Realtime;

namespace Api.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSignalR();
        services.AddSingleton<IDispatchRealtimeNotifier, SignalRDispatchRealtimeNotifier>();
        services.AddMemoryCache();
        services.AddScoped<CurrentUserContext>();
        services.AddScoped<UserVisibilityPolicy>();
        services.AddScoped<SystemResourceVisibilityPolicy>();
        services.AddScoped<AccessPermissionPolicy>();

        var groqOptions = LoadGroqOptions();
        if (groqOptions is null)
        {
            services.AddSingleton<ILlmClient, UnconfiguredLlmClient>();
        }
        else
        {
            services.AddSingleton(groqOptions);
            services.AddHttpClient<ILlmClient, GroqLlmClient>(client =>
            {
                client.BaseAddress = new Uri(groqOptions.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(groqOptions.TimeoutSeconds);
            });
        }
        var mapOptions = LoadMapServicesOptions();
        services.AddSingleton(mapOptions);
        services.AddScoped<GeocodingRefreshService>();
        services.AddHttpClient<IGeocodingService, NominatimGeocodingService>(client =>
        {
            client.BaseAddress = new Uri(mapOptions.NominatimBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(mapOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(mapOptions.UserAgent);
        });
        services.AddHttpClient<IHospitalFinder, OverpassHospitalFinder>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(mapOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(mapOptions.UserAgent);
        });
        services.AddHttpClient<IMapRoutingService, OsrmMapRoutingService>(client =>
        {
            client.BaseAddress = new Uri(mapOptions.OsrmBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(mapOptions.TimeoutSeconds);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(mapOptions.UserAgent);
        });
        services.AddHttpClient("ViaCep", client =>
        {
            client.BaseAddress = new Uri("https://viacep.com.br/");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        // --- Resend ---
        var resendApiKey = EnvLoader.GetEnv("RESEND_API_KEY");
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = resendApiKey;
        });
        services.AddTransient<ResendClient>();

        // Serviço de reset de senha
        services.AddScoped<PasswordResetEmailService>();

        return services;
    }

    private static GroqOptions? LoadGroqOptions()
    {
        var apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY")?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
            return null;

        var timeoutValue = Environment.GetEnvironmentVariable("GROQ_TIMEOUT_SECONDS");
        var timeout = int.TryParse(timeoutValue, out var parsedTimeout) && parsedTimeout is >= 1 and <= 120
            ? parsedTimeout
            : GroqOptions.DefaultTimeoutSeconds;

        return new GroqOptions
        {
            ApiKey = apiKey,
            BaseUrl = Environment.GetEnvironmentVariable("GROQ_BASE_URL")?.Trim()
                ?? GroqOptions.DefaultBaseUrl,
            Model = Environment.GetEnvironmentVariable("GROQ_MODEL")?.Trim()
                ?? GroqOptions.DefaultModel,
            TimeoutSeconds = timeout
        };
    }

    private static MapServicesOptions LoadMapServicesOptions()
    {
        var timeoutValue = Environment.GetEnvironmentVariable("MAP_HTTP_TIMEOUT_SECONDS");
        var timeout = int.TryParse(timeoutValue, out var parsedTimeout) && parsedTimeout is >= 3 and <= 60
            ? parsedTimeout
            : MapServicesOptions.DefaultTimeoutSeconds;

        var configuredOverpassUrls = Environment.GetEnvironmentVariable("OVERPASS_BASE_URLS")?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var legacyOverpassUrl = Environment.GetEnvironmentVariable("OVERPASS_BASE_URL")?.Trim();
        var overpassUrls = configuredOverpassUrls is { Length: > 0 }
            ? configuredOverpassUrls
            : !string.IsNullOrWhiteSpace(legacyOverpassUrl)
                ? new[] { legacyOverpassUrl, MapServicesOptions.DefaultOverpassFallbackBaseUrl }
                : new[] { MapServicesOptions.DefaultOverpassBaseUrl, MapServicesOptions.DefaultOverpassFallbackBaseUrl };

        return new MapServicesOptions
        {
            OverpassBaseUrls = overpassUrls.Select(EnsureTrailingSlash).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            OsrmBaseUrl = EnsureTrailingSlash(
                Environment.GetEnvironmentVariable("OSRM_BASE_URL")?.Trim()
                ?? MapServicesOptions.DefaultOsrmBaseUrl),
            NominatimBaseUrl = EnsureTrailingSlash(
                Environment.GetEnvironmentVariable("NOMINATIM_BASE_URL")?.Trim()
                ?? MapServicesOptions.DefaultNominatimBaseUrl),
            TimeoutSeconds = timeout,
            UserAgent = Environment.GetEnvironmentVariable("MAP_USER_AGENT")?.Trim()
                ?? "FHO-Emergency-Academic-Prototype/1.0"
        };
    }

    private static string EnsureTrailingSlash(string value) => value.EndsWith('/') ? value : $"{value}/";
}
