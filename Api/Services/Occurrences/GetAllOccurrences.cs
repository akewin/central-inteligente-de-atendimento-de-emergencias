using Api.Dtos;
using Api.Helpers.Pagination;
using Api.Interfaces.Repositories;
using Api.Validations;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Occurrences;

public class GetAllOccurrences
{
    private readonly IOccurrenceRepository _repository;
    private readonly OccurrenceValidator _validator;

    public GetAllOccurrences(
        IOccurrenceRepository repository,
        OccurrenceValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public Task<PagedResult<OccurrenceListDto>> ExecuteAsync(
        int page,
        int pageSize,
        string? status,
        string? search)
    {
        var parsedStatus = _validator.ValidateList(page, pageSize, status, search);
        var query = _repository.Query();

        if (parsedStatus.HasValue)
            query = query.Where(o => o.Status == parsedStatus.Value);

        var normalizedSearch = search?.Trim();
        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(o =>
                EF.Functions.ILike(o.Description, $"%{normalizedSearch}%") ||
                EF.Functions.ILike(o.LocationDescription, $"%{normalizedSearch}%"));
        }

        var dtoQuery = query
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Select(o => new OccurrenceListDto
            {
                Id = o.Id,
                DescriptionSummary = o.Description.Length <= 160
                    ? o.Description
                    : o.Description.Substring(0, 157) + "...",
                LocationDescription = o.LocationDescription,
                Status = o.Status.ToString(),
                ConfirmedType = o.ConfirmedType.HasValue ? o.ConfirmedType.Value.ToString() : null,
                ConfirmedPriority = o.ConfirmedPriority.HasValue ? o.ConfirmedPriority.Value.ToString() : null,
                CreatedBy = new OccurrenceCreatorDto
                {
                    Id = o.CreatedByUser.Id,
                    Username = o.CreatedByUser.Username,
                    FullName = o.CreatedByUser.FullName
                },
                CreatedAt = o.CreatedAt
            });

        return PagedResult<OccurrenceListDto>.CreateAsync(dtoQuery, page, pageSize);
    }
}
