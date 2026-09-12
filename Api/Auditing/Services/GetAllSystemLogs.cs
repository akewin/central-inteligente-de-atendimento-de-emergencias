using Api.Dtos;
using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;
using Api.Validations;

namespace Api.Auditing.Services;

public class GetAllSystemLogs
{
    private readonly ISystemLogRepository _repository;

    public GetAllSystemLogs(ISystemLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<SystemLogListDto>> ExecuteAsync(
        int? userId = null,
        string? action = null,
        string? startDate = null,
        string? endDate = null,
        int page = 1,
        int pageSize = 10)
    {
        DateTime? startDateTime = null;
        DateTime? endDateTime = null;

        if (!string.IsNullOrWhiteSpace(startDate)
            && DateTime.TryParse(startDate, out var parsedStart))
        {
            startDateTime = DateTime.SpecifyKind(
                parsedStart.Date,
                DateTimeKind.Utc
            );
        }

        if (!string.IsNullOrWhiteSpace(endDate)
            && DateTime.TryParse(endDate, out var parsedEnd))
        {
            endDateTime = DateTime.SpecifyKind(
                parsedEnd.Date.AddDays(1),
                DateTimeKind.Utc
            );
        }

        ValidateDateRange.EnsureValidPeriod(startDateTime, endDateTime);

        return await _repository.GetPagedAsync(
            userId,
            action,
            startDateTime,
            endDateTime,
            page,
            pageSize
        );
    }
}
