using System.Net;
using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;
using Api.Validations;
using Api.Geography;
using Api.Services.Geography;
using Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Units;

public sealed class UpdateUnit
{
    private readonly IUnitRepository _repository;
    private readonly UnitValidator _validator;
    private readonly ApiDbContext _context;
    private readonly CreateSystemLog _audit;
    private readonly IGeocodingService _geocoding;

    public UpdateUnit(IUnitRepository repository, UnitValidator validator, ApiDbContext context, CreateSystemLog audit, IGeocodingService geocoding)
        => (_repository, _validator, _context, _audit, _geocoding) = (repository, validator, context, audit, geocoding);

    public async Task<UnitReadDto> ExecuteAsync(int id, UnitUpdateDto? dto, CancellationToken cancellationToken)
    {
        var input = _validator.Validate(dto, requireConcurrencyToken: true);
        var unit = await _repository.GetByIdAsync(id, tracked: true, cancellationToken)
            ?? throw new AppException("Equipe não encontrada.", (int)HttpStatusCode.NotFound);

        if (await _repository.NameExistsAsync(input.NormalizedName, id, cancellationToken))
            throw new AppException("Já existe uma equipe com este nome.", (int)HttpStatusCode.Conflict);

        var service = await _repository.GetServiceAsync(input.Service, cancellationToken)
            ?? throw new AppException("Serviço de emergência não encontrado.", (int)HttpStatusCode.Conflict);
        var activeDispatch = await _context.Dispatches.AsNoTracking()
            .Where(dispatch => dispatch.UnitId == unit.Id
                && dispatch.Status != DispatchStatus.CONCLUIDO
                && dispatch.Status != DispatchStatus.CANCELADO)
            .Select(dispatch => (DispatchStatus?)dispatch.Status)
            .SingleOrDefaultAsync(cancellationToken);
        if (activeDispatch.HasValue && unit.EmergencyService.Type != service.Type)
            throw new AppException("O órgão de uma equipe não pode ser alterado enquanto ela possui um chamado ativo.", (int)HttpStatusCode.Conflict);
        var coordinates = await _geocoding.GeocodeAsync(new GeocodingAddress(
            input.PostalCode, input.Street, input.Number, input.Complement, input.Neighborhood, input.City, input.State), cancellationToken);
        if (!GeographicDistanceCalculator.AreValidCoordinates(coordinates.Latitude, coordinates.Longitude))
            throw new AppException("Não foi possível determinar a localização da equipe.", (int)HttpStatusCode.UnprocessableEntity);

        var previous = new { unit.Name, Service = unit.EmergencyService.Type.ToString(), Status = unit.Status.ToString() };
        _context.Entry(unit).Property(item => item.UpdatedAt).OriginalValue = input.ExpectedUpdatedAt!.Value.ToUniversalTime();
        unit.Name = input.Name;
        unit.NormalizedName = input.NormalizedName;
        unit.EmergencyServiceId = service.Id;
        unit.EmergencyService = service;
        unit.PostalCode = input.PostalCode; unit.Street = input.Street; unit.Number = input.Number;
        unit.Complement = input.Complement; unit.Neighborhood = input.Neighborhood; unit.City = input.City;
        unit.State = input.State; unit.AddressReference = input.Reference; unit.GeocodingSource = coordinates.Source;
        unit.Latitude = coordinates.Latitude;
        unit.Longitude = coordinates.Longitude;
        unit.Status = activeDispatch switch
        {
            DispatchStatus.ATRIBUIDO => UnitStatus.RESERVADA,
            DispatchStatus.ACEITO => UnitStatus.DESLOCAMENTO,
            DispatchStatus.NO_LOCAL or DispatchStatus.EM_ATENDIMENTO => UnitStatus.EM_ATENDIMENTO,
            _ => input.Status
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new AppException("A equipe foi alterada por outro usuário. Recarregue os dados e tente novamente.", (int)HttpStatusCode.Conflict);
        }
        catch (DbUpdateException)
        {
            throw new AppException("Não foi possível atualizar a equipe; verifique se o nome já está em uso.", (int)HttpStatusCode.Conflict);
        }

        await _audit.ExecuteAsync(SystemLogActionFactory.Update("Unit", unit.Id), data: new SystemLogDataDto
        {
            Type = "update",
            PrevState = previous,
            CurrState = new { unit.Name, Service = service.Type.ToString(), Status = unit.Status.ToString() }
        });
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return UnitMapper.ToReadDto(unit);
    }
}
