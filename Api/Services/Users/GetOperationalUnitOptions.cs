using Api.Data;
using Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Users;

public sealed class GetOperationalUnitOptions
{
    private readonly ApiDbContext _context;
    public GetOperationalUnitOptions(ApiDbContext context) => _context = context;

    public async Task<IReadOnlyList<OperationalUnitSummaryDto>> ExecuteAsync(int? userId, CancellationToken cancellationToken)
        => await _context.Units.AsNoTracking()
            .Include(unit => unit.EmergencyService)
            .Where(unit => unit.OperationalUser == null || unit.OperationalUser.Id == userId)
            .OrderBy(unit => unit.EmergencyService.Type).ThenBy(unit => unit.Name)
            .Select(unit => new OperationalUnitSummaryDto
            {
                Id = unit.Id,
                Name = unit.Name,
                Service = unit.EmergencyService.Type.ToString()
            })
            .ToListAsync(cancellationToken);
}
