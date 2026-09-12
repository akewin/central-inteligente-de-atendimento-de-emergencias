using Api.AI.Contracts;
using Api.AI.Interfaces;

namespace Api.Tests.AI;

public sealed class FakeLlmClient : ILlmClient
{
    public LlmResponse Response { get; set; } = new(
        """{"type":"ACIDENTE_TRANSITO","priority":"ALTA","recommendedServices":["POLICIA","SAMU"],"reason":"Acidente simulado com possível vítima."}""",
        "FakeProvider",
        "fake-model");

    public Exception? Exception { get; set; }
    public LlmRequest? LastRequest { get; private set; }

    public Task<LlmResponse> GenerateStructuredAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LastRequest = request;
        if (Exception is not null) throw Exception;
        return Task.FromResult(Response);
    }
}
