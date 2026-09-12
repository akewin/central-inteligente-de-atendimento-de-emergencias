using Api.AI.Contracts;
using Api.Validations;

namespace Api.Tests.AI;

public sealed class AIAnalysisValidatorTests
{
    private readonly AIAnalysisValidator _validator = new();

    [Fact]
    public void ValidateAndCreate_WithValidResponse_ReturnsValidatedEntity()
    {
        var response = Response("""{"type":"INCENDIO","priority":"CRITICA","recommendedServices":["BOMBEIROS","SAMU"],"reason":" Incêndio com risco às pessoas. "}""");

        var analysis = _validator.ValidateAndCreate(response, 42);

        Assert.Equal(42, analysis.OccurrenceId);
        Assert.True(analysis.RecommendsFireDepartment);
        Assert.True(analysis.RecommendsSamu);
        Assert.False(analysis.RecommendsPolice);
        Assert.Equal("Incêndio com risco às pessoas.", analysis.Reason);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-json")]
    [InlineData("{}")]
    [InlineData("{\"type\":\"DESCONHECIDO\",\"priority\":\"ALTA\",\"recommendedServices\":[\"SAMU\"],\"reason\":\"Motivo\"}")]
    [InlineData("{\"type\":\"FERIMENTO\",\"priority\":\"URGENTE\",\"recommendedServices\":[\"SAMU\"],\"reason\":\"Motivo\"}")]
    [InlineData("{\"type\":\"FERIMENTO\",\"priority\":\"ALTA\",\"recommendedServices\":[],\"reason\":\"Motivo\"}")]
    [InlineData("{\"type\":\"FERIMENTO\",\"priority\":\"ALTA\",\"recommendedServices\":[\"SAMU\",\"SAMU\"],\"reason\":\"Motivo\"}")]
    [InlineData("{\"type\":\"FERIMENTO\",\"priority\":\"ALTA\",\"recommendedServices\":[\"HOSPITAL\"],\"reason\":\"Motivo\"}")]
    [InlineData("{\"type\":\"FERIMENTO\",\"priority\":\"ALTA\",\"recommendedServices\":[\"SAMU\"],\"reason\":\"   \"}")]
    [InlineData("{\"type\":\"FERIMENTO\",\"priority\":\"ALTA\",\"recommendedServices\":[\"SAMU\"],\"reason\":\"Motivo\",\"extra\":true}")]
    public void ValidateAndCreate_WithUntrustedInvalidOutput_RejectsIt(string content)
    {
        Assert.ThrowsAny<Exception>(() => _validator.ValidateAndCreate(Response(content), 1));
    }

    [Theory]
    [InlineData(null, "model")]
    [InlineData("provider", null)]
    public void ValidateAndCreate_WithMissingTechnicalIdentity_RejectsIt(
        string? provider,
        string? model)
    {
        var response = new LlmResponse(
            """{"type":"OUTROS","priority":"BAIXA","recommendedServices":["POLICIA"],"reason":"Motivo"}""",
            provider,
            model);

        Assert.ThrowsAny<Exception>(() => _validator.ValidateAndCreate(response, 1));
    }

    private static LlmResponse Response(string content) =>
        new(content, "FakeProvider", "fake-model");
}
