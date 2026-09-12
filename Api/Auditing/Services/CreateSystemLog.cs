using Api.Interfaces.Repositories;
using Api.Models;
using Api.Security.Jwt;

namespace Api.Auditing.Services;

public class CreateSystemLog
{
    private readonly ISystemLogRepository _repository;
    private readonly CurrentUserContext _currentUser;

    public CreateSystemLog(
        ISystemLogRepository repository,
        CurrentUserContext currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task ExecuteAsync(
      string action,
      int? userId = null,
      string? generatedBy = null,
      object? data = null
    )
    {
        var log = new SystemLog
        {
            UserId = userId ?? _currentUser.GetId(),
            Action = action,
            Data = data != null
            ? SystemLogDataSerializer.Serialize(data)
            : null,
            GeneratedBy = generatedBy ?? _currentUser.GetUsername(),
            IpAddress = _currentUser.GetIpAddress(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.CreateAsync(log);
    }
}
