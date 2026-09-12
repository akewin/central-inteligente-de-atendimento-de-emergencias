using Api.Data;
using Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.EmergencyServices;

public sealed class GetEmergencyServices
{
    private readonly ApiDbContext _context;
    public GetEmergencyServices(ApiDbContext context) => _context = context;

    public Task<List<EmergencyServiceReadDto>> ExecuteAsync(CancellationToken cancellationToken = default)
        => _context.EmergencyServices.AsNoTracking().OrderBy(service => service.Id).Select(service => new EmergencyServiceReadDto
        {
            Id = service.Id,
            Type = service.Type.ToString(),
            EmergencyNumber = service.EmergencyNumber,
            DisplayName = service.DisplayName
        }).ToListAsync(cancellationToken);
}
