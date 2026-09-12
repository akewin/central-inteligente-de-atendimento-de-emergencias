namespace Api.AI.Exceptions;

public sealed class LlmNotConfiguredException : InvalidOperationException
{
    public LlmNotConfiguredException()
        : base("O serviço de inteligência artificial não está configurado.")
    {
    }
}
