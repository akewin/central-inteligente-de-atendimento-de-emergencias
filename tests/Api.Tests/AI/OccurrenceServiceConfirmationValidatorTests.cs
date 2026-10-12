using Api.Dtos;
using Api.Models.Enums;
using Api.Validations;

namespace Api.Tests.AI;

public sealed class OccurrenceServiceConfirmationValidatorTests
{
    private readonly OccurrenceServiceConfirmationValidator _validator = new();

    [Fact]
    public void Validate_WithAllowedUniqueServices_ReturnsSelection()
    {
        var result = _validator.Validate(new OccurrenceServiceConfirmationRequestDto
        {
            ConfirmedServices = ["GUARDA_CIVIL", "DEFESA_CIVIL"]
        });

        Assert.Equal(2, result.Count);
        Assert.Contains(EmergencyServiceType.GUARDA_CIVIL, result);
        Assert.Contains(EmergencyServiceType.DEFESA_CIVIL, result);
    }

    [Theory]
    [InlineData()]
    [InlineData("SAMU", "SAMU")]
    [InlineData("HOSPITAL")]
    [InlineData("samu")]
    public void Validate_WithInvalidSelection_RejectsIt(params string[] services)
    {
        Assert.ThrowsAny<Exception>(() => _validator.Validate(
            new OccurrenceServiceConfirmationRequestDto { ConfirmedServices = services }));
    }
}
