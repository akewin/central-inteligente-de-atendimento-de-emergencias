using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Models;
using Api.Validations;

namespace Api.Services.SystemResources;

public class CreateSystemResource
{
    private readonly ISystemResourceRepository _repository;
    private readonly ApiDbContext _context;
    private readonly CreateSystemLog _createSystemLog;

    public CreateSystemResource(
        ISystemResourceRepository repository,
        ApiDbContext context,
        CreateSystemLog createSystemLog)
    {
        _repository = repository;
        _context = context;
        _createSystemLog = createSystemLog;
    }

    public async Task<SystemResourceReadDto> ExecuteAsync(SystemResourceCreateDto dto)
    {
        Guard.AgainstNull(dto, "Payload inválido para criação de SystemResource");

        if (await _repository.ExistsByNameAsync(dto.Name))
            throw new AppException("Já existe um recurso com esse nome.");

        var resource = new SystemResource
        {
            Name = dto.Name,
            ExhibitionName = dto.ExhibitionName
        };

        await _repository.CreateAsync(resource);
        await _context.SaveChangesAsync();

        await _createSystemLog.ExecuteAsync(
            action: SystemLogActionFactory.Create("SystemResource", resource.Id),
            data: new SystemLogDataDto
            {
                Type = "create",
                Created = new
                {
                    resource.Name,
                    resource.ExhibitionName
                }
            }
        );

        return new SystemResourceReadDto
        {
            Id = resource.Id,
            Name = resource.Name,
            ExhibitionName = resource.ExhibitionName
        };
    }
}
