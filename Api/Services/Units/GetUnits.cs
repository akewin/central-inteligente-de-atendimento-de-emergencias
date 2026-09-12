using Api.Dtos;
using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;
using Api.Models.Enums;

namespace Api.Services.Units;

public sealed class GetUnits
{
    private readonly IUnitRepository _repository;
    public GetUnits(IUnitRepository repository) => _repository = repository;

    public async Task<PagedResult<UnitReadDto>> ExecuteAsync(int page, int pageSize, string? search, string? service, string? status, CancellationToken cancellationToken)
    {
        if (page <= 0 || pageSize is <= 0 or > 100)
            throw new AppException("Página e tamanho da página devem ser positivos; o limite é 100 itens.");

        EmergencyServiceType? serviceFilter = null;
        if (!string.IsNullOrWhiteSpace(service))
        {
            if (!Enum.TryParse<EmergencyServiceType>(service, true, out var parsed) || !Enum.IsDefined(parsed))
                throw new AppException("Filtro de serviço inválido.");
            serviceFilter = parsed;
        }

        UnitStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<UnitStatus>(status, true, out var parsed) || !Enum.IsDefined(parsed))
                throw new AppException("Filtro de status inválido.");
            statusFilter = parsed;
        }

        var result = await _repository.GetPagedAsync(page, pageSize, search, serviceFilter, statusFilter, cancellationToken);
        return new PagedResult<UnitReadDto>(result.TotalItems, result.Page, result.PageSize, result.Data.Select(UnitMapper.ToReadDto).ToList());
    }
}
