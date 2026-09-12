using Api.Auditing;
using Api.Auditing.Services;
using Api.Interfaces.Repositories;
using Api.Middlewares;
using Api.Validations;

namespace Api.Services.SystemResources;

public class DeleteSystemResource
{
    private readonly ISystemResourceRepository _repository;
    private readonly CreateSystemLog _createSystemLog;

    public DeleteSystemResource(
        ISystemResourceRepository repository,
        CreateSystemLog createSystemLog)
    {
        _repository = repository;
        _createSystemLog = createSystemLog;
    }

    public async Task ExecuteAsync(int id)
    {
        Guard.AgainstNonPositiveInt(id);

        var deleted = await _repository.SoftDeleteAsync(id);

        if (!deleted)
            throw new AppException("Recurso não encontrado.");

        await _createSystemLog.ExecuteAsync(
            action: SystemLogActionFactory.Delete("SystemResource", id)
        );
    }
}
