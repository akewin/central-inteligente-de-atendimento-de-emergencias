using System.Net;
using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;
using System.Text.RegularExpressions;

namespace Api.Validations;

public sealed class UnitValidator
{
    public ValidatedUnitInput Validate(UnitCreateDto? dto, bool requireConcurrencyToken = false)
    {
        if (dto is null)
            throw new AppException("Os dados da equipe são obrigatórios.");

        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length is < 2 or > 50)
            throw new AppException("O nome da equipe deve ter entre 2 e 50 caracteres.");

        if (!Enum.TryParse<EmergencyServiceType>(dto.Service?.Trim(), true, out var service) ||
            !Enum.IsDefined(service))
            throw new AppException("Serviço inválido. Use POLICIA, SAMU ou BOMBEIROS.");

        if (!Enum.TryParse<UnitStatus>(dto.Status?.Trim(), true, out var status) ||
            !Enum.IsDefined(status))
            throw new AppException("Status inválido.");
        if (status == UnitStatus.RESERVADA)
            throw new AppException("O status RESERVADA é controlado exclusivamente pelo despacho.");

        var postalCode = Regex.Replace(dto.PostalCode ?? string.Empty, "[^0-9]", string.Empty);
        if (postalCode.Length != 8) throw new AppException("O CEP deve possuir 8 dígitos.");
        var street = Required(dto.Street, "Rua", 200);
        var number = Required(dto.Number, "Número", 20);
        var neighborhood = Required(dto.Neighborhood, "Bairro", 100);
        var city = Required(dto.City, "Cidade", 100);
        var state = Required(dto.State, "UF", 2).ToUpperInvariant();
        if (!Regex.IsMatch(state, "^[A-Z]{2}$")) throw new AppException("A UF deve possuir exatamente duas letras.");
        var complement = Optional(dto.Complement, "Complemento", 100);
        var reference = Optional(dto.Reference, "Ponto de referência", 200);

        DateTime? expectedUpdatedAt = null;
        if (requireConcurrencyToken)
        {
            expectedUpdatedAt = (dto as UnitUpdateDto)?.ExpectedUpdatedAt;
            if (expectedUpdatedAt is null)
                throw new AppException("A versão atual da equipe é obrigatória.");

            if (expectedUpdatedAt.Value.Kind == DateTimeKind.Unspecified)
                expectedUpdatedAt = DateTime.SpecifyKind(expectedUpdatedAt.Value, DateTimeKind.Utc);
        }

        return new ValidatedUnitInput(
            name,
            name.ToUpperInvariant(),
            service,
            postalCode, street, number, complement, neighborhood, city, state, reference,
            status,
            expectedUpdatedAt);
    }

    private static string Required(string? value, string field, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) throw new AppException($"{field} é obrigatório.");
        if (normalized.Length > maxLength) throw new AppException($"{field} deve possuir no máximo {maxLength} caracteres.");
        return normalized;
    }

    private static string? Optional(string? value, string field, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (normalized?.Length > maxLength) throw new AppException($"{field} deve possuir no máximo {maxLength} caracteres.");
        return normalized;
    }
}

public sealed record ValidatedUnitInput(
    string Name,
    string NormalizedName,
    EmergencyServiceType Service,
    string PostalCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string? Reference,
    UnitStatus Status,
    DateTime? ExpectedUpdatedAt);
