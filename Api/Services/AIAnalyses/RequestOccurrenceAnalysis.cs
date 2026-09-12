using System.Net;
using Api.AI.Contracts;
using Api.AI.Exceptions;
using Api.AI.Interfaces;
using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;
using Api.Models.Enums;
using Api.Validations;

namespace Api.Services.AIAnalyses;

public sealed class RequestOccurrenceAnalysis
{
    private const string SystemPrompt = """
        Você é uma ferramenta de apoio à decisão para um protótipo acadêmico de gerenciamento de emergências.
        Analise somente a descrição fornecida. Você não atende chamadas, não acessa bancos de dados,
        não despacha equipes e não toma decisões finais.
        Recomende exatamente: type; priority; recommendedServices; reason.
        type deve ser ACIDENTE_TRANSITO, INCENDIO, AGRESSAO, ROUBO, FERIMENTO, MAL_SUBITO,
        PESSOA_DESAPARECIDA, RESGATE ou OUTROS.
        priority deve ser BAIXA, MEDIA, ALTA ou CRITICA.
        recommendedServices deve ser uma lista não vazia e sem duplicatas contendo somente POLICIA,
        SAMU e/ou BOMBEIROS.
        reason deve ser uma justificativa objetiva em português e não pode afirmar que a decisão foi
        confirmada ou que um despacho foi realizado.
        Retorne somente JSON compatível com o schema fornecido, sem markdown, comentários ou campos adicionais.
        """;

    private const string ResponseSchema = """
        {"type":"object","additionalProperties":false,"required":["type","priority","recommendedServices","reason"],"properties":{"type":{"type":"string","enum":["ACIDENTE_TRANSITO","INCENDIO","AGRESSAO","ROUBO","FERIMENTO","MAL_SUBITO","PESSOA_DESAPARECIDA","RESGATE","OUTROS"]},"priority":{"type":"string","enum":["BAIXA","MEDIA","ALTA","CRITICA"]},"recommendedServices":{"type":"array","minItems":1,"maxItems":3,"items":{"type":"string","enum":["POLICIA","SAMU","BOMBEIROS"]}},"reason":{"type":"string","minLength":1,"maxLength":1000}}}
        """;

    private readonly IOccurrenceRepository _occurrences;
    private readonly IAIAnalysisRepository _analyses;
    private readonly ILlmClient _llmClient;
    private readonly AIAnalysisValidator _validator;
    private readonly ApiDbContext _context;
    private readonly CreateSystemLog _createSystemLog;

    public RequestOccurrenceAnalysis(
        IOccurrenceRepository occurrences,
        IAIAnalysisRepository analyses,
        ILlmClient llmClient,
        AIAnalysisValidator validator,
        ApiDbContext context,
        CreateSystemLog createSystemLog)
    {
        _occurrences = occurrences;
        _analyses = analyses;
        _llmClient = llmClient;
        _validator = validator;
        _context = context;
        _createSystemLog = createSystemLog;
    }

    public async Task<AIAnalysisReadDto> ExecuteAsync(
        int occurrenceId,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositiveInt(occurrenceId);
        var occurrence = await _occurrences.GetForAnalysisAsync(occurrenceId, cancellationToken)
            ?? throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        EnsureStatusAllowsAnalysis(occurrence);

        LlmResponse response;
        try
        {
            response = await _llmClient.GenerateStructuredAsync(
                new LlmRequest(
                    SystemPrompt,
                    $"Descrição da ocorrência:\n{occurrence.Description}",
                    ResponseSchema,
                    MaxOutputTokens: 500),
                cancellationToken);
        }
        catch (LlmNotConfiguredException)
        {
            throw new AppException(
                "O serviço de inteligência artificial ainda não está configurado.",
                (int)HttpStatusCode.ServiceUnavailable);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new AppException("O serviço de inteligência artificial excedeu o tempo limite.",
                (int)HttpStatusCode.GatewayTimeout);
        }
        catch (LlmProviderException exception)
        {
            var statusCode = exception.StatusCode == (int)HttpStatusCode.TooManyRequests
                ? (int)HttpStatusCode.ServiceUnavailable
                : (int)HttpStatusCode.BadGateway;
            throw new AppException(exception.Message, statusCode);
        }

        var analysis = _validator.ValidateAndCreate(response, occurrenceId);

        await _context.Entry(occurrence).ReloadAsync(cancellationToken);
        EnsureStatusAllowsAnalysis(occurrence);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        occurrence.Status = OccurrenceStatus.AGUARDANDO_CONFIRMACAO;
        await _analyses.CreateAsync(analysis);
        await _context.SaveChangesAsync(cancellationToken);

        await _createSystemLog.ExecuteAsync(
            action: SystemLogActionFactory.Create("AIAnalysis", analysis.Id),
            data: new SystemLogDataDto
            {
                Type = "create",
                Created = new
                {
                    analysis.Id,
                    analysis.OccurrenceId,
                    RecommendedType = analysis.RecommendedType.ToString(),
                    RecommendedPriority = analysis.RecommendedPriority.ToString(),
                    RecommendedServices = AIAnalysisMapper.ToReadDto(analysis).RecommendedServices,
                    analysis.Provider,
                    analysis.Model
                }
            });

        await transaction.CommitAsync(cancellationToken);
        return AIAnalysisMapper.ToReadDto(analysis);
    }

    private static void EnsureStatusAllowsAnalysis(Api.Models.Occurrence occurrence)
    {
        if (occurrence.ServicesConfirmedAt.HasValue ||
            occurrence.Status is not OccurrenceStatus.ABERTA and not OccurrenceStatus.AGUARDANDO_CONFIRMACAO)
            throw new AppException(
                "O status atual da ocorrência não permite solicitar análise.",
                (int)HttpStatusCode.Conflict);
    }
}
