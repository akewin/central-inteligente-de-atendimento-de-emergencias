using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Validations;

namespace Api.Services.SystemResources;

public class GetSystemResourceById
{
    private readonly ISystemResourceRepository _repository;

    public GetSystemResourceById(ISystemResourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemResourceReadDto> ExecuteAsync(int id)
    {
        Guard.AgainstNonPositiveInt(id);

        var resource = await _repository.GetByIdAsync(id)
          ?? throw new AppException("Recurso não encontrado.");

        return new SystemResourceReadDto
        {
            Id = resource.Id,
            Name = resource.Name,
            ExhibitionName = resource.ExhibitionName
        };
    }
}
