namespace Api.AI.Configuration;

public sealed class GroqOptions
{
    public const string DefaultBaseUrl = "https://api.groq.com/openai/v1/";
    public const string DefaultModel = "openai/gpt-oss-20b";
    public const int DefaultTimeoutSeconds = 15;

    public required string ApiKey { get; init; }
    public string BaseUrl { get; init; } = DefaultBaseUrl;
    public string Model { get; init; } = DefaultModel;
    public int TimeoutSeconds { get; init; } = DefaultTimeoutSeconds;
}
