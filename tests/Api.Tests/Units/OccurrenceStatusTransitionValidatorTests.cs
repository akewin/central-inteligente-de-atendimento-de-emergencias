using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Validations;

namespace Api.Tests.Units;

public sealed class OccurrenceStatusTransitionValidatorTests
{
    private readonly OccurrenceStatusTransitionValidator _validator = new();

    [Fact]
    public void Validate_AcceptsCancellationWithReasonAndNormalizesUtc()
    {
        var result = _validator.Validate(new OccurrenceStatusTransitionRequestDto
        {
            TargetStatus = "CANCELADA", ExpectedUpdatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
            Reason = "  Ocorrência duplicada  "
        });

        Assert.Equal(OccurrenceStatus.CANCELADA, result.TargetStatus);
        Assert.Equal(DateTimeKind.Utc, result.ExpectedUpdatedAt.Kind);
        Assert.Equal("Ocorrência duplicada", result.Reason);
    }

    [Theory]
    [InlineData("DESPACHADA", "não permitido")]
    [InlineData("DESCONHECIDA", "não permitido")]
    [InlineData("CANCELADA", "abc")]
    public void Validate_RejectsInvalidHumanCommand(string status, string reason)
        => Assert.Throws<AppException>(() => _validator.Validate(new OccurrenceStatusTransitionRequestDto
        {
            TargetStatus = status, ExpectedUpdatedAt = DateTime.UtcNow, Reason = reason
        }));
}
