using Api.Dtos;
using Api.Middlewares;
using Api.Validations;

namespace Api.Tests.Units;

public sealed class DispatchValidatorTests
{
    private readonly DispatchValidator _validator = new();

    [Fact]
    public void Validate_ReturnsStableOrderedIds_WhenRequestIsValid()
    {
        var result = _validator.Validate(new DispatchConfirmationRequestDto { UnitIds = [9, 2, 5] });

        Assert.Equal([2, 5, 9], result);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public void Validate_RejectsInvalidRequests(DispatchConfirmationRequestDto? request)
        => Assert.Throws<AppException>(() => _validator.Validate(request));

    public static TheoryData<DispatchConfirmationRequestDto?> InvalidRequests => new()
    {
        null,
        new DispatchConfirmationRequestDto { UnitIds = [] },
        new DispatchConfirmationRequestDto { UnitIds = [1, 1] },
        new DispatchConfirmationRequestDto { UnitIds = [0] },
        new DispatchConfirmationRequestDto { UnitIds = [1, 2, 3, 4] }
    };
}
