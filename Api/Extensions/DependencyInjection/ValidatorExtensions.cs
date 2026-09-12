using System.Reflection;
using Api.Validations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Extensions.DependencyInjection;

public static class ValidatorExtensions
{
    public static IServiceCollection AddValidators(
        this IServiceCollection services,
        ILogger logger
    )
    {
        // Obtém apenas o assembly onde estão os Validators
        var assembly = typeof(UserValidator).Assembly;

        var validatorTypes = assembly
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Namespace != null &&
                t.Namespace.StartsWith("Api.Validations") &&
                t.Name.EndsWith("Validator"));

        foreach (var type in validatorTypes)
        {
            services.AddScoped(type);
        }

        logger.LogInformation("Validators registrados com sucesso");

        return services;
    }
}