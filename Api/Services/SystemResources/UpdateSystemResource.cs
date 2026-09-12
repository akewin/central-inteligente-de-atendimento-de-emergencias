using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Validations;

namespace Api.Services.SystemResources;

public class UpdateSystemResource
{
    private readonly ISystemResourceRepository _repository;
    private readonly ApiDbContext _context;
    private readonly CreateSystemLog _createSystemLog;

    public UpdateSystemResource(
        ISystemResourceRepository repository,
        ApiDbContext context,
        CreateSystemLog createSystemLog)
    {
        _repository = repository;
        _context = context;
        _createSystemLog = createSystemLog;
    }

    public async Task<SystemResourceReadDto> ExecuteAsync(int id, SystemResourceUpdateDto dto)
    {
        Guard.AgainstNonPositiveInt(id);
        Guard.AgainstMismatchedIds(id, dto.Id);

        var resource = await _repository.GetByIdAsync(id)
            ?? throw new AppException("Recurso não encontrado.");

        var prevState = new
        {
            resource.Name,
            resource.ExhibitionName
        };

        if (!string.IsNullOrWhiteSpace(dto.Name))
            resource.Name = dto.Name;

        if (!string.IsNullOrWhiteSpace(dto.ExhibitionName))
            resource.ExhibitionName = dto.ExhibitionName;

        await _repository.UpdateAsync(resource);
        await _context.SaveChangesAsync();

        var currState = new
        {
            resource.Name,
            resource.ExhibitionName
        };

        await _createSystemLog.ExecuteAsync(
            action: SystemLogActionFactory.Update("SystemResource", resource.Id),
            data: new SystemLogDataDto
            {
                Type = "update",
                PrevState = prevState,
                CurrState = currState
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
