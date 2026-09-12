using Api.Middlewares;
using Api.Models.Enums;
using Api.Services.Occurrences;

namespace Api.Tests.Units;

public sealed class OccurrenceStateMachineTests
{
    private readonly OccurrenceStateMachine _machine = new();

    [Theory]
    [InlineData(OccurrenceStatus.ABERTA, OccurrenceStatus.EM_ANALISE)]
    [InlineData(OccurrenceStatus.EM_ANALISE, OccurrenceStatus.AGUARDANDO_CONFIRMACAO)]
    [InlineData(OccurrenceStatus.EM_ANALISE, OccurrenceStatus.ABERTA)]
    [InlineData(OccurrenceStatus.AGUARDANDO_CONFIRMACAO, OccurrenceStatus.DESPACHADA)]
    [InlineData(OccurrenceStatus.DESPACHADA, OccurrenceStatus.EM_ATENDIMENTO)]
    [InlineData(OccurrenceStatus.EM_ATENDIMENTO, OccurrenceStatus.FINALIZADA)]
    [InlineData(OccurrenceStatus.DESPACHADA, OccurrenceStatus.CANCELADA)]
    public void EnsureAllowed_AcceptsDefinedTransitions(OccurrenceStatus current, OccurrenceStatus target)
        => _machine.EnsureAllowed(current, target);

    [Theory]
    [InlineData(OccurrenceStatus.DESPACHADA, OccurrenceStatus.FINALIZADA)]
    [InlineData(OccurrenceStatus.FINALIZADA, OccurrenceStatus.EM_ATENDIMENTO)]
    [InlineData(OccurrenceStatus.CANCELADA, OccurrenceStatus.ABERTA)]
    public void EnsureAllowed_RejectsInvalidTransitions(OccurrenceStatus current, OccurrenceStatus target)
        => Assert.Throws<AppException>(() => _machine.EnsureAllowed(current, target));
}
