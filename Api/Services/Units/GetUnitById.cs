using System.Net;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;

namespace Api.Services.Units;

public sealed class GetUnitById
{
    private readonly IUnitRepository _repository;
    public GetUnitById(IUnitRepository repository) => _repository = repository;

    public async Task<UnitReadDto> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var unit = await _repository.GetByIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new AppException("Equipe não encontrada.", (int)HttpStatusCode.NotFound);
        return UnitMapper.ToReadDto(unit);
    }
}
