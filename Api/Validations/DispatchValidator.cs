using Api.Dtos;
using Api.Middlewares;

namespace Api.Validations;

public sealed class DispatchValidator
{
    public IReadOnlyList<int> Validate(DispatchConfirmationRequestDto? dto)
    {
        if (dto?.UnitIds is null || dto.UnitIds.Count is < 1 or > 3)
            throw new AppException("Selecione entre uma e três equipes.");
        if (dto.UnitIds.Any(id => id <= 0))
            throw new AppException("Todos os identificadores de equipe devem ser positivos.");
        if (dto.UnitIds.Distinct().Count() != dto.UnitIds.Count)
            throw new AppException("A seleção não pode conter equipes duplicadas.");
        return dto.UnitIds.OrderBy(id => id).ToArray();
    }
}
