using Api.AI.Contracts;
using Api.AI.Exceptions;
using Api.AI.Interfaces;
using Api.AI.Providers;

namespace Api.Tests.AI;

public sealed class UnconfiguredLlmClientTests
{
    [Fact]
    public async Task GenerateStructuredAsync_WhenProviderIsNotConfigured_FailsSafely()
    {
        ILlmClient client = new UnconfiguredLlmClient();
        var request = new LlmRequest("Instruções", "Conteúdo");

        var exception = await Assert.ThrowsAsync<LlmNotConfiguredException>(
            () => client.GenerateStructuredAsync(request));

        Assert.Equal(
            "O serviço de inteligência artificial não está configurado.",
            exception.Message);
    }

    [Fact]
    public async Task GenerateStructuredAsync_WhenCancelled_PropagatesCancellation()
    {
        ILlmClient client = new UnconfiguredLlmClient();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.GenerateStructuredAsync(
                new LlmRequest("Instruções", "Conteúdo"),
                cancellation.Token));
    }

    [Fact]
    public async Task GenerateStructuredAsync_WhenRequestIsNull_RejectsIt()
    {
        ILlmClient client = new UnconfiguredLlmClient();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.GenerateStructuredAsync(null!));
    }
}
