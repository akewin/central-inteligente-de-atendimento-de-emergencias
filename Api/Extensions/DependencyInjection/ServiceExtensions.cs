using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Api.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        ILogger logger
    )
    {
        var assembly = typeof(Program).Assembly;
        int count = 0;

        var types = assembly.GetTypes().Where(t =>
            t.IsClass &&
            !t.IsAbstract &&
            !t.IsGenericType &&
            !t.IsNested &&
            !typeof(Delegate).IsAssignableFrom(t) &&
            t.Namespace != null &&
            (
                t.Namespace.StartsWith("Api.Services") ||
                t.Namespace.StartsWith("Api.Security.Auth") ||
                t.Namespace.StartsWith("Api.Security.RefreshTokens") ||
                t.Namespace.StartsWith("Api.Auditing.Services")
            )
        );

        foreach (var type in types)
        {
            services.AddScoped(type);
            count++;
        }

        logger.LogInformation("{count} Services registrados com sucesso", count);

        return services;
    }
}
