using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Api.AI.Configuration;
using Api.AI.Contracts;
using Api.AI.Exceptions;
using Api.AI.Interfaces;

namespace Api.AI.Providers;

public sealed class GroqLlmClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly GroqOptions _options;

    public GroqLlmClient(HttpClient httpClient, GroqOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<LlmResponse> GenerateStructuredAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.ResponseSchema))
            throw new ArgumentException("Um schema de resposta é obrigatório.", nameof(request));

        JsonElement schema;
        try
        {
            schema = JsonSerializer.Deserialize<JsonElement>(request.ResponseSchema);
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("O schema de resposta não contém JSON válido.", nameof(request), exception);
        }

        var payload = new
        {
            model = _options.Model,
            messages = new[]
            {
                new { role = "system", content = request.Instructions },
                new { role = "user", content = request.Content }
            },
            temperature = 0.1,
            reasoning_effort = "low",
            max_completion_tokens = request.MaxOutputTokens,
            response_format = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "occurrence_analysis",
                    strict = true,
                    schema
                }
            }
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(message, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var requestId = ReadRequestId(response);

        if (!response.IsSuccessStatusCode)
            throw new LlmProviderException(
                ProviderErrorMessage((int)response.StatusCode),
                (int)response.StatusCode,
                requestId);

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var content = root.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                throw new JsonException("Resposta sem conteúdo.");

            var model = root.TryGetProperty("model", out var modelElement)
                ? modelElement.GetString()
                : _options.Model;
            var responseId = root.TryGetProperty("id", out var idElement)
                ? idElement.GetString()
                : requestId;

            return new LlmResponse(content, "Groq", model ?? _options.Model, responseId);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException
            or IndexOutOfRangeException or KeyNotFoundException)
        {
            throw new LlmProviderException(
                "A Groq retornou uma resposta que não pôde ser interpretada.",
                (int)response.StatusCode,
                requestId) { Source = exception.Source };
        }
    }

    private static string? ReadRequestId(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-request-id", out var values))
            return values.FirstOrDefault();
        return null;
    }

    private static string ProviderErrorMessage(int statusCode) => statusCode switch
    {
        401 or 403 => "A autenticação com a Groq falhou.",
        429 => "O limite gratuito da Groq foi atingido. Tente novamente mais tarde.",
        >= 500 => "A Groq está temporariamente indisponível.",
        _ => "A Groq recusou a solicitação de análise."
    };
}
