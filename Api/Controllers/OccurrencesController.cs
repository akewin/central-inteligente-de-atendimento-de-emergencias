using Api.Dtos;
using Api.Services.Occurrences;
using Api.Services.AIAnalyses;
using Api.Services.Units;
using Api.Services.Dispatches;
using Api.Services.Maps;
using Api.Services.Hospitals;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/occurrences")]
public class OccurrencesController : ControllerBase
{
    private readonly CreateOccurrence _create;
    private readonly GetAllOccurrences _getAll;
    private readonly GetOccurrenceById _getById;
    private readonly RequestOccurrenceAnalysis _requestAnalysis;
    private readonly ConfirmOccurrenceServices _confirmServices;
    private readonly RecommendUnitsForOccurrence _recommendUnits;
    private readonly ConfirmDispatch _confirmDispatch;
    private readonly TransitionOccurrenceStatus _transitionStatus;
    private readonly GetOccurrenceTimeline _getTimeline;

    public OccurrencesController(
        CreateOccurrence create,
        GetAllOccurrences getAll,
        GetOccurrenceById getById,
        RequestOccurrenceAnalysis requestAnalysis,
        ConfirmOccurrenceServices confirmServices,
        RecommendUnitsForOccurrence recommendUnits,
        ConfirmDispatch confirmDispatch,
        TransitionOccurrenceStatus transitionStatus,
        GetOccurrenceTimeline getTimeline)
    {
        _create = create;
        _getAll = getAll;
        _getById = getById;
        _requestAnalysis = requestAnalysis;
        _confirmServices = confirmServices;
        _recommendUnits = recommendUnits;
        _confirmDispatch = confirmDispatch;
        _transitionStatus = transitionStatus;
        _getTimeline = getTimeline;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OccurrenceCreateDto? dto)
    {
        var occurrence = await _create.ExecuteAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = occurrence.Id }, occurrence);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var occurrences = await _getAll.ExecuteAsync(page, pageSize, status, search);
        return Ok(occurrences);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var occurrence = await _getById.ExecuteAsync(id);
        return Ok(occurrence);
    }

    [HttpPost("{id:int}/analysis")]
    public async Task<IActionResult> RequestAnalysis(
        int id,
        CancellationToken cancellationToken)
    {
        var analysis = await _requestAnalysis.ExecuteAsync(id, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, analysis);
    }

    [HttpPut("{id:int}/service-confirmation")]
    public async Task<IActionResult> ConfirmServices(
        int id,
        [FromBody] OccurrenceServiceConfirmationRequestDto? dto,
        CancellationToken cancellationToken)
    {
        var confirmation = await _confirmServices.ExecuteAsync(id, dto, cancellationToken);
        return Ok(confirmation);
    }

    [HttpGet("{id:int}/unit-recommendations")]
    public async Task<IActionResult> RecommendUnits(
        int id,
        [FromQuery] int limitPerService = 3,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await _recommendUnits.ExecuteAsync(id, limitPerService, cancellationToken);
        return Ok(recommendations);
    }

    [HttpPost("{id:int}/dispatches")]
    public async Task<IActionResult> ConfirmDispatch(
        int id,
        [FromBody] DispatchConfirmationRequestDto? dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _confirmDispatch.ExecuteAsync(id, dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> TransitionStatus(
        int id,
        [FromBody] OccurrenceStatusTransitionRequestDto? dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _transitionStatus.ExecuteAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}/timeline")]
    public async Task<IActionResult> GetTimeline(
        int id,
        CancellationToken cancellationToken = default)
        => Ok(await _getTimeline.ExecuteAsync(id, cancellationToken));

    [HttpGet("{id:int}/dispatches")]
    public async Task<IActionResult> GetDispatches(
        int id,
        [FromServices] GetOccurrenceDispatches service,
        CancellationToken cancellationToken = default)
        => Ok(await service.ExecuteAsync(id, cancellationToken));

    [HttpGet("{id:int}/map")]
    public async Task<IActionResult> GetMap(
        int id,
        [FromServices] GetOccurrenceMap service,
        [FromQuery] int radiusKm = 15,
        [FromQuery] int hospitalLimit = 8,
        [FromQuery] int? hospitalId = null,
        CancellationToken cancellationToken = default)
        => Ok(await service.ExecuteAsync(id, radiusKm, hospitalLimit, hospitalId, cancellationToken));

    [HttpPut("{id:int}/hospital")]
    public async Task<IActionResult> ConfirmHospital(
        int id,
        [FromBody] HospitalSelectionRequestDto? dto,
        [FromServices] ConfirmOccurrenceHospital service,
        CancellationToken cancellationToken = default)
        => Ok(await service.ExecuteAsync(id, dto, cancellationToken));

    [HttpGet("{id:int}/transport")]
    public async Task<IActionResult> GetTransport(int id, [FromServices] CentralTransport service, CancellationToken cancellationToken)
        => Ok(await service.GetAsync(id, cancellationToken));

    [HttpPost("{id:int}/transport/samu")]
    public async Task<IActionResult> AssignSamu(int id, [FromBody] AssignSamuDto dto, [FromServices] CentralTransport service, CancellationToken cancellationToken)
        => Ok(await service.AssignSamuAsync(id, dto, cancellationToken));
}
