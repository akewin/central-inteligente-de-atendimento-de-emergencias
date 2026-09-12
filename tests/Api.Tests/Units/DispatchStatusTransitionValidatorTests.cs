using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Validations;

namespace Api.Tests.Units;

public sealed class DispatchStatusTransitionValidatorTests
{
    private readonly DispatchStatusTransitionValidator _validator = new();
    private static readonly DateTime Version = new(2026, 8, 31, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(DispatchStatus.ATRIBUIDO, "ACEITO")]
    [InlineData(DispatchStatus.ACEITO, "NO_LOCAL")]
    [InlineData(DispatchStatus.NO_LOCAL, "EM_ATENDIMENTO")]
    [InlineData(DispatchStatus.EM_ATENDIMENTO, "CONCLUIDO")]
    public void Validate_AcceptsOnlyNextSequentialState(DispatchStatus current, string target)
    {
        var result = _validator.Validate(current, new OperationalDispatchTransitionRequestDto
        { TargetStatus = target, ExpectedUpdatedAt = Version });
        Assert.Equal(target, result.Target.ToString());
        Assert.Equal(Version, result.ExpectedUpdatedAt);
    }

    [Theory]
    [InlineData(DispatchStatus.ATRIBUIDO, "EM_ATENDIMENTO")]
    [InlineData(DispatchStatus.ACEITO, "ACEITO")]
    [InlineData(DispatchStatus.CONCLUIDO, "ACEITO")]
    [InlineData(DispatchStatus.CANCELADO, "ACEITO")]
    public void Validate_RejectsJumpsRepeatsAndTerminalStates(DispatchStatus current, string target)
        => Assert.Throws<AppException>(() => _validator.Validate(current, new OperationalDispatchTransitionRequestDto
        { TargetStatus = target, ExpectedUpdatedAt = Version }));

    [Fact]
    public void Validate_RejectsOperatorCancellation()
        => Assert.Throws<AppException>(() => _validator.Validate(DispatchStatus.ATRIBUIDO,
            new OperationalDispatchTransitionRequestDto { TargetStatus = "CANCELADO", ExpectedUpdatedAt = Version }));

    [Fact]
    public void Validate_RequiresConcurrencyVersion()
        => Assert.Throws<AppException>(() => _validator.Validate(DispatchStatus.ATRIBUIDO,
            new OperationalDispatchTransitionRequestDto { TargetStatus = "ACEITO" }));
}
