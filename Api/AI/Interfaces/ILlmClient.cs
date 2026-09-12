using Api.AI.Contracts;

namespace Api.AI.Interfaces;

public interface ILlmClient
{
    Task<LlmResponse> GenerateStructuredAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default);
}
