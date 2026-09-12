using Api.Dtos;
using Api.Middlewares;
using Api.Models.Enums;

namespace Api.Validations;

public sealed class OccurrenceServiceConfirmationValidator
{
    public HashSet<EmergencyServiceType> Validate(
        OccurrenceServiceConfirmationRequestDto? dto)
    {
        if (dto?.ConfirmedServices is not { Length: >= 1 and <= 3 })
            throw new AppException("Selecione pelo menos um órgão válido.");

        var services = new HashSet<EmergencyServiceType>();
        foreach (var value in dto.ConfirmedServices)
        {
            if (!Enum.TryParse<EmergencyServiceType>(value, ignoreCase: false, out var service) ||
                !Enum.IsDefined(service) ||
                !services.Add(service))
                throw new AppException("A lista de órgãos confirmados é inválida.");
        }

        return services;
    }
}
