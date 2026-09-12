using System.Net;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;

namespace Api.Services.Occurrences;

public class GetOccurrenceById
{
    private readonly IOccurrenceRepository _repository;

    public GetOccurrenceById(IOccurrenceRepository repository)
    {
        _repository = repository;
    }

    public async Task<OccurrenceReadDto> ExecuteAsync(int id)
    {
        Guard.AgainstNonPositiveInt(id);

        var occurrence = await _repository.GetByIdAsync(id)
            ?? throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        return OccurrenceMapper.ToReadDto(occurrence);
    }
}
