using System.Net;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.UnitOperations;

public sealed class GetMyDispatchById
{
    private readonly ApiDbContext _context;
    private readonly OperationalUserResolver _operationalUser;
    public GetMyDispatchById(ApiDbContext context, OperationalUserResolver operationalUser)
        => (_context, _operationalUser) = (context, operationalUser);

    public async Task<OperationalDispatchReadDto> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) throw new AppException("Identificador inválido.");
        var user = await _operationalUser.ResolveAsync(cancellationToken);
        var dispatch = await _context.Dispatches.AsNoTracking()
            .Include(item => item.Unit).ThenInclude(unit => unit.EmergencyService)
            .Include(item => item.Occurrence)
                .ThenInclude(occurrence => occurrence.PatientTransports).ThenInclude(transport => transport.Hospital)
            .Include(item => item.Occurrence)
                .ThenInclude(occurrence => occurrence.PatientTransports).ThenInclude(transport => transport.HospitalWard)
            .SingleOrDefaultAsync(item => item.Id == id && item.UnitId == user.UnitId, cancellationToken)
            ?? throw new AppException("Chamado não encontrado para a equipe autenticada.", (int)HttpStatusCode.NotFound);
        return OperationalDispatchMapper.ToDto(dispatch);
    }
}
