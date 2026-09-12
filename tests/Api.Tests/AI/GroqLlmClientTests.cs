using System.Net;
using System.Text;
using System.Text.Json;
using Api.AI.Configuration;
using Api.AI.Contracts;
using Api.AI.Exceptions;
using Api.AI.Providers;

namespace Api.Tests.AI;

public sealed class GroqLlmClientTests
{
    private const string Schema =
        """{"type":"object","additionalProperties":false,"required":["type"],"properties":{"type":{"type":"string"}}}""";

    [Fact]
    public async Task GenerateStructuredAsync_SendsStrictSchemaAndMapsResponse()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedBody = null;
        var handler = new StubHandler(async request =>
        {
            capturedRequest = request;
            capturedBody = await request.Content!.ReadAsStringAsync();
            return JsonResponse(HttpStatusCode.OK,
                """{"id":"req-123","model":"openai/gpt-oss-20b","choices":[{"message":{"content":"{\"type\":\"INCENDIO\"}"}}]}""");
        });
        var client = CreateClient(handler);

        var result = await client.GenerateStructuredAsync(
            new LlmRequest("Classifique", "Há fumaça", Schema, 100));

        Assert.Equal("{\"type\":\"INCENDIO\"}", result.Content);
        Assert.Equal("Groq", result.Provider);
        Assert.Equal("openai/gpt-oss-20b", result.Model);
        Assert.Equal("req-123", result.RequestId);
        Assert.Equal("Bearer", capturedRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("test-key", capturedRequest.Headers.Authorization.Parameter);

        using var payload = JsonDocument.Parse(capturedBody!);
        Assert.True(payload.RootElement.GetProperty("response_format")
            .GetProperty("json_schema").GetProperty("strict").GetBoolean());
        Assert.Equal("low", payload.RootElement.GetProperty("reasoning_effort").GetString());
        Assert.Equal(100, payload.RootElement.GetProperty("max_completion_tokens").GetInt32());
    }

    [Fact]
    public async Task GenerateStructuredAsync_WhenFreeTierLimitIsReached_ReturnsSpecificFailure()
    {
        var handler = new StubHandler(_ => Task.FromResult(
            JsonResponse(HttpStatusCode.TooManyRequests, "{\"error\":{}}")));
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<LlmProviderException>(() =>
            client.GenerateStructuredAsync(new LlmRequest("Classifique", "Teste", Schema)));

        Assert.Equal(429, exception.StatusCode);
        Assert.Contains("limite gratuito", exception.Message);
    }

    [Fact]
    public async Task GenerateStructuredAsync_WhenProviderPayloadIsMalformed_FailsSafely()
    {
        var handler = new StubHandler(_ => Task.FromResult(
            JsonResponse(HttpStatusCode.OK, "{\"choices\":[]}")));
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<LlmProviderException>(() =>
            client.GenerateStructuredAsync(new LlmRequest("Classifique", "Teste", Schema)));
    }

    private static GroqLlmClient CreateClient(HttpMessageHandler handler) => new(
        new HttpClient(handler) { BaseAddress = new Uri(GroqOptions.DefaultBaseUrl) },
        new GroqOptions { ApiKey = "test-key" });

    private static HttpResponseMessage JsonResponse(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private sealed class StubHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => responseFactory(request);
    }
}
