using Api.AI.Contracts;
using Api.AI.Exceptions;
using Api.AI.Interfaces;

namespace Api.AI.Providers;

public sealed class UnconfiguredLlmClient : ILlmClient
{
    public Task<LlmResponse> GenerateStructuredAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        throw new LlmNotConfiguredException();
    }
}
