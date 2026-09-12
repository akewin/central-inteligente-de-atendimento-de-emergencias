using System.Net;
using System.Text.Json;
using Api.AI.Contracts;
using Api.Dtos;
using Api.Middlewares;
using Api.Models;
using Api.Models.Enums;

namespace Api.Validations;

public sealed class AIAnalysisValidator
{
    private static readonly HashSet<string> AllowedServices =
        new(StringComparer.Ordinal) { "POLICIA", "SAMU", "BOMBEIROS" };

    public AIAnalysis ValidateAndCreate(LlmResponse response, int occurrenceId)
    {
        if (response is null || string.IsNullOrWhiteSpace(response.Content))
            throw InvalidResponse();

        AIAnalysisLlmOutputDto? output;
        try
        {
            output = JsonSerializer.Deserialize<AIAnalysisLlmOutputDto>(response.Content);
        }
        catch (JsonException)
        {
            throw InvalidResponse();
        }

        if (output is null ||
            !TryParseDefined(output.Type, out OccurrenceType type) ||
            !TryParseDefined(output.Priority, out OccurrencePriority priority))
            throw InvalidResponse();

        if (output.RecommendedServices is not { Length: >= 1 and <= 3 })
            throw InvalidResponse();

        var uniqueServices = new HashSet<string>(StringComparer.Ordinal);
        foreach (var service in output.RecommendedServices)
        {
            if (string.IsNullOrWhiteSpace(service) ||
                !AllowedServices.Contains(service) ||
                !uniqueServices.Add(service))
                throw InvalidResponse();
        }

        var reason = output.Reason?.Trim();
        if (string.IsNullOrWhiteSpace(reason) || reason.Length > 1000)
            throw InvalidResponse();

        var provider = response.Provider?.Trim();
        var model = response.Model?.Trim();
        if (string.IsNullOrWhiteSpace(provider) || provider.Length > 50 ||
            string.IsNullOrWhiteSpace(model) || model.Length > 100)
            throw InvalidResponse();

        return new AIAnalysis
        {
            OccurrenceId = occurrenceId,
            RecommendedType = type,
            RecommendedPriority = priority,
            RecommendsPolice = uniqueServices.Contains("POLICIA"),
            RecommendsSamu = uniqueServices.Contains("SAMU"),
            RecommendsFireDepartment = uniqueServices.Contains("BOMBEIROS"),
            Reason = reason,
            Provider = provider,
            Model = model
        };
    }

    private static bool TryParseDefined<TEnum>(string? value, out TEnum parsed)
        where TEnum : struct, Enum
    {
        return Enum.TryParse(value, ignoreCase: false, out parsed) &&
               Enum.IsDefined(parsed);
    }

    private static AppException InvalidResponse() =>
        new("O provedor de IA retornou uma resposta inválida.", (int)HttpStatusCode.BadGateway);
}
