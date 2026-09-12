namespace Api.AI.Contracts;

public sealed record LlmRequest(
    string Instructions,
    string Content,
    string? ResponseSchema = null,
    int? MaxOutputTokens = null);
