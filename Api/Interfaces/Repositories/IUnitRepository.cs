using Api.Helpers.Pagination;
using Api.Models;
using Api.Models.Enums;

namespace Api.Interfaces.Repositories;

public interface IUnitRepository
{
    Task<Unit> CreateAsync(Unit unit, CancellationToken cancellationToken = default);
    Task<Unit?> GetByIdAsync(int id, bool tracked = false, CancellationToken cancellationToken = default);
    Task<EmergencyService?> GetServiceAsync(EmergencyServiceType type, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string normalizedName, int? exceptId = null, CancellationToken cancellationToken = default);
    Task<PagedResult<Unit>> GetPagedAsync(int page, int pageSize, string? search, EmergencyServiceType? service, UnitStatus? status, CancellationToken cancellationToken = default);
    Task<List<EmergencyService>> GetServicesAsync(IReadOnlyCollection<EmergencyServiceType> services, CancellationToken cancellationToken = default);
    Task<List<Unit>> GetAvailableByServicesAsync(IReadOnlyCollection<EmergencyServiceType> services, CancellationToken cancellationToken = default);
}
