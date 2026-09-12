using Api.Dtos;
using Api.Models;

namespace Api.Mappers;

public static class UnitMapper
{
    public static UnitReadDto ToReadDto(Unit unit) => new()
    {
        Id = unit.Id,
        Name = unit.Name,
        PostalCode = unit.PostalCode,
        Street = unit.Street,
        Number = unit.Number,
        Complement = unit.Complement,
        Neighborhood = unit.Neighborhood,
        City = unit.City,
        State = unit.State,
        Reference = unit.AddressReference,
        Address = FormatAddress(unit),
        Status = unit.Status.ToString(),
        CreatedAt = unit.CreatedAt,
        UpdatedAt = unit.UpdatedAt,
        Service = new EmergencyServiceReadDto
        {
            Id = unit.EmergencyService.Id,
            Type = unit.EmergencyService.Type.ToString(),
            EmergencyNumber = unit.EmergencyService.EmergencyNumber,
            DisplayName = unit.EmergencyService.DisplayName
        }
    };

    public static string FormatAddress(Unit unit)
    {
        if (string.IsNullOrWhiteSpace(unit.Street)) return "Localização cadastrada anteriormente";
        var parts = new[] { $"{unit.Street}, {unit.Number}", unit.Neighborhood, $"{unit.City} - {unit.State}", unit.PostalCode is null ? null : $"CEP {unit.PostalCode[..5]}-{unit.PostalCode[5..]}" };
        return string.Join(" · ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}
