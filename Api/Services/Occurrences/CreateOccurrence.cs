using System.Net;
using Api.Auditing;
using Api.Auditing.Services;
using Api.Data;
using Api.Dtos;
using Api.Interfaces.Repositories;
using Api.Mappers;
using Api.Middlewares;
using Api.Models;
using Api.Security.Jwt;
using Api.Validations;
using Api.Services.Geography;
using Api.Geography;
using System.Text.RegularExpressions;

namespace Api.Services.Occurrences;

public class CreateOccurrence
{
    private readonly IOccurrenceRepository _repository;
    private readonly ApiDbContext _context;
    private readonly OccurrenceValidator _validator;
    private readonly CurrentUserContext _currentUser;
    private readonly CreateSystemLog _createSystemLog;
    private readonly IGeocodingService _geocoding;

    public CreateOccurrence(
        IOccurrenceRepository repository,
        ApiDbContext context,
        OccurrenceValidator validator,
        CurrentUserContext currentUser,
        CreateSystemLog createSystemLog,
        IGeocodingService geocoding)
    {
        _repository = repository;
        _context = context;
        _validator = validator;
        _currentUser = currentUser;
        _createSystemLog = createSystemLog;
        _geocoding = geocoding;
    }

    public async Task<OccurrenceReadDto> ExecuteAsync(OccurrenceCreateDto? dto)
    {
        _validator.ValidateCreate(dto);

        var creatorId = _currentUser.GetId()
            ?? throw new AppException("Usuário não autenticado.", (int)HttpStatusCode.Unauthorized);

        if (!await _repository.CreatorExistsAsync(creatorId))
            throw new AppException("Usuário autenticado não encontrado ou inativo.", (int)HttpStatusCode.Unauthorized);

        var postalCode = Regex.Replace(dto!.PostalCode!, "[^0-9]", string.Empty);
        var address = new GeocodingAddress(
            postalCode, dto.Street!.Trim(), dto.Number!.Trim(), dto.Complement?.Trim(),
            dto.Neighborhood!.Trim(), dto.City!.Trim(), dto.State!.Trim().ToUpperInvariant());
        var coordinates = await _geocoding.GeocodeAsync(address);
        if (!GeographicDistanceCalculator.AreValidCoordinates(coordinates.Latitude, coordinates.Longitude))
            throw new AppException("Não foi possível localizar o endereço informado.", (int)HttpStatusCode.UnprocessableEntity);
        var locationDescription = FormatAddress(address);
        if (locationDescription.Length > 500)
            throw new AppException("O endereço completo deve possuir no máximo 500 caracteres.");

        var occurrence = new Occurrence
        {
            Description = dto.Description!.Trim(),
            LocationDescription = locationDescription,
            PostalCode = postalCode,
            Street = address.Street,
            Number = address.Number,
            Complement = string.IsNullOrWhiteSpace(address.Complement) ? null : address.Complement,
            Neighborhood = address.Neighborhood,
            City = address.City,
            State = address.State,
            AddressReference = string.IsNullOrWhiteSpace(dto.Reference) ? null : dto.Reference.Trim(),
            GeocodingSource = coordinates.Source,
            Latitude = coordinates.Latitude,
            Longitude = coordinates.Longitude,
            CreatedByUserId = creatorId
        };

        await _repository.CreateAsync(occurrence);
        await _context.SaveChangesAsync();

        await _createSystemLog.ExecuteAsync(
            action: SystemLogActionFactory.Create("Occurrence", occurrence.Id),
            data: new SystemLogDataDto
            {
                Type = "create",
                Created = new
                {
                    occurrence.Id,
                    Status = occurrence.Status.ToString(),
                    occurrence.CreatedByUserId
                }
            });

        var persisted = await _repository.GetByIdAsync(occurrence.Id)
            ?? throw new InvalidOperationException("Ocorrência criada não foi encontrada.");

        return OccurrenceMapper.ToReadDto(persisted);
    }

    private static string FormatAddress(GeocodingAddress address)
    {
        var complement = string.IsNullOrWhiteSpace(address.Complement) ? string.Empty : $", {address.Complement}";
        return $"{address.Street}, {address.Number}{complement} - {address.Neighborhood}, {address.City}/{address.State} - CEP {address.PostalCode[..5]}-{address.PostalCode[5..]}";
    }
}
