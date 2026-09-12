using System.Net;
using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;
using Api.Models;
using Api.Validations;
using Api.Geography;
using Api.Services.Geography;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Units;

public sealed class CreateUnit
{
    private readonly IUnitRepository _repository;
    private readonly UnitValidator _validator;
    private readonly ApiDbContext _context;
    private readonly CreateSystemLog _audit;
    private readonly IGeocodingService _geocoding;

    public CreateUnit(IUnitRepository repository, UnitValidator validator, ApiDbContext context, CreateSystemLog audit, IGeocodingService geocoding)
        => (_repository, _validator, _context, _audit, _geocoding) = (repository, validator, context, audit, geocoding);

    public async Task<UnitReadDto> ExecuteAsync(UnitCreateDto? dto, CancellationToken cancellationToken)
    {
        var input = _validator.Validate(dto);
        if (await _repository.NameExistsAsync(input.NormalizedName, cancellationToken: cancellationToken))
            throw new AppException("Já existe uma equipe com este nome.", (int)HttpStatusCode.Conflict);

        var service = await _repository.GetServiceAsync(input.Service, cancellationToken)
            ?? throw new AppException("Serviço de emergência não encontrado.", (int)HttpStatusCode.Conflict);
        var coordinates = await _geocoding.GeocodeAsync(ToAddress(input), cancellationToken);
        if (!GeographicDistanceCalculator.AreValidCoordinates(coordinates.Latitude, coordinates.Longitude))
            throw new AppException("Não foi possível determinar a localização da equipe.", (int)HttpStatusCode.UnprocessableEntity);

        var unit = new Unit
        {
            Name = input.Name,
            NormalizedName = input.NormalizedName,
            EmergencyServiceId = service.Id,
            EmergencyService = service,
            PostalCode = input.PostalCode, Street = input.Street, Number = input.Number,
            Complement = input.Complement, Neighborhood = input.Neighborhood, City = input.City,
            State = input.State, AddressReference = input.Reference, GeocodingSource = coordinates.Source,
            Latitude = coordinates.Latitude,
            Longitude = coordinates.Longitude,
            Status = input.Status
        };

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        await _repository.CreateAsync(unit, cancellationToken);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new AppException("Não foi possível criar a equipe; verifique se o nome já está em uso.", (int)HttpStatusCode.Conflict);
        }

        await _audit.ExecuteAsync(SystemLogActionFactory.Create("Unit", unit.Id), data: new SystemLogDataDto
        {
            Type = "create",
            Created = new { unit.Id, unit.Name, Service = service.Type.ToString(), Status = unit.Status.ToString() }
        });
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return UnitMapper.ToReadDto(unit);
    }

    private static GeocodingAddress ToAddress(ValidatedUnitInput input) => new(
        input.PostalCode, input.Street, input.Number, input.Complement, input.Neighborhood, input.City, input.State);
}
