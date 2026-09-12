using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Validations;

namespace Api.Tests.Units;

public sealed class UnitValidatorTests
{
    private readonly UnitValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_NormalizesAndParsesValues()
    {
        var result = _validator.Validate(new UnitCreateDto
        {
            Name = " samu-01 ", Service = "samu", PostalCode = "01001-000", Street = "Praça da Sé",
            Number = "100", Neighborhood = "Sé", City = "São Paulo", State = "sp", Status = "disponivel"
        });

        Assert.Equal("SAMU-01", result.NormalizedName);
        Assert.Equal(EmergencyServiceType.SAMU, result.Service);
        Assert.Equal(UnitStatus.DISPONIVEL, result.Status);
    }

    [Theory]
    [InlineData(null, "POLICIA", "01001000", "SP", "DISPONIVEL")]
    [InlineData("PM-01", "INVALIDO", "01001000", "SP", "DISPONIVEL")]
    [InlineData("PM-01", "POLICIA", "123", "SP", "DISPONIVEL")]
    [InlineData("PM-01", "POLICIA", "01001000", "S", "DISPONIVEL")]
    [InlineData("PM-01", "POLICIA", "01001000", "SP", "INVALIDO")]
    public void Validate_WithInvalidInput_Throws(string? name, string service, string postalCode, string state, string status)
    {
        Assert.Throws<AppException>(() => _validator.Validate(new UnitCreateDto
        {
            Name = name, Service = service, PostalCode = postalCode, Street = "Rua A", Number = "1",
            Neighborhood = "Centro", City = "São Paulo", State = state, Status = status
        }));
    }

    [Fact]
    public void ValidateUpdate_WithoutExpectedVersion_Throws()
    {
        Assert.Throws<AppException>(() => _validator.Validate(new UnitUpdateDto
        {
            Name = "PM-01", Service = "POLICIA", PostalCode = "01001000", Street = "Rua A", Number = "1",
            Neighborhood = "Centro", City = "São Paulo", State = "SP", Status = "DISPONIVEL"
        }, requireConcurrencyToken: true));
    }
}
