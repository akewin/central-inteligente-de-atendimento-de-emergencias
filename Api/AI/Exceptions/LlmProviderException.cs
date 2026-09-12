namespace Api.AI.Exceptions;

public sealed class LlmProviderException : Exception
{
    public LlmProviderException(string message, int? statusCode = null, string? requestId = null)
        : base(message)
    {
        StatusCode = statusCode;
        RequestId = requestId;
    }

    public int? StatusCode { get; }
    public string? RequestId { get; }
}
