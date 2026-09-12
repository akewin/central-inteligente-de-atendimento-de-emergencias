using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;
using System.Text.RegularExpressions;

namespace Api.Validations;

public class OccurrenceValidator
{
    public void ValidateCreate(OccurrenceCreateDto? dto)
    {
        Guard.AgainstNull(dto, "Payload inválido para criação de ocorrência.");

        Guard.AgainstNullOrEmpty(dto!.Description, "Descrição");
        if (dto.Description!.Trim().Length > 4000)
            throw new AppException("A descrição deve possuir no máximo 4000 caracteres.");

        ValidateRequired(dto.PostalCode, "CEP", 9);
        var postalCode = Regex.Replace(dto.PostalCode!, "[^0-9]", string.Empty);
        if (postalCode.Length != 8) throw new AppException("O CEP deve possuir 8 dígitos.");
        ValidateRequired(dto.Street, "Rua", 200);
        ValidateRequired(dto.Number, "Número", 20);
        ValidateRequired(dto.Neighborhood, "Bairro", 100);
        ValidateRequired(dto.City, "Cidade", 100);
        ValidateRequired(dto.State, "UF", 2);
        if (!Regex.IsMatch(dto.State!.Trim(), "^[A-Za-z]{2}$"))
            throw new AppException("A UF deve possuir exatamente duas letras.");
        ValidateOptional(dto.Complement, "Complemento", 100);
        ValidateOptional(dto.Reference, "Ponto de referência", 200);
    }

    private static void ValidateRequired(string? value, string field, int maxLength)
    {
        Guard.AgainstNullOrEmpty(value, field);
        if (value!.Trim().Length > maxLength)
            throw new AppException($"{field} deve possuir no máximo {maxLength} caracteres.");
    }

    private static void ValidateOptional(string? value, string field, int maxLength)
    {
        if (value?.Trim().Length > maxLength)
            throw new AppException($"{field} deve possuir no máximo {maxLength} caracteres.");
    }

    public OccurrenceStatus? ValidateList(
        int page,
        int pageSize,
        string? status,
        string? search)
    {
        Guard.AgainstNonPositiveInt(page);
        Guard.AgainstNonPositiveInt(pageSize);

        if (pageSize > 100)
            throw new AppException("O tamanho da página deve ser de no máximo 100 itens.");

        if (search?.Trim().Length > 100)
            throw new AppException("A busca deve possuir no máximo 100 caracteres.");

        if (string.IsNullOrWhiteSpace(status))
            return null;

        if (!Enum.TryParse<OccurrenceStatus>(status, false, out var parsedStatus) ||
            !Enum.IsDefined(parsedStatus))
        {
            throw new AppException("Status de ocorrência inválido.");
        }

        return parsedStatus;
    }
}
