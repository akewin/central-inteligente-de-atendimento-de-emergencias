using System.Reflection;
using Api.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Extensions.DependencyInjection;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        ILogger logger
    )
    {
        var assembly = typeof(UserRepository).Assembly;

        var repositoryTypes = assembly
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Namespace != null &&
                t.Namespace.StartsWith("Api.Repositories") &&
                t.Name.EndsWith("Repository"));

        foreach (var implementationType in repositoryTypes)
        {
            var interfaceType = implementationType
                .GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }

        logger.LogInformation("Repositories registrados com sucesso");

        return services;
    }
}