namespace Api.AI.Contracts;

public sealed record LlmResponse(
    string Content,
    string? Provider = null,
    string? Model = null,
    string? RequestId = null);
