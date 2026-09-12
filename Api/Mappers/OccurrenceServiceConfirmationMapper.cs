using Api.Dtos;
using Api.Models;

namespace Api.Mappers;

public static class OccurrenceServiceConfirmationMapper
{
    public static OccurrenceServiceConfirmationReadDto? ToReadDto(Occurrence occurrence)
    {
        if (!occurrence.ServicesConfirmedAt.HasValue || occurrence.ServicesConfirmedByUser is null)
            return null;

        var services = new List<string>(3);
        if (occurrence.PoliceConfirmed) services.Add("POLICIA");
        if (occurrence.SamuConfirmed) services.Add("SAMU");
        if (occurrence.FireDepartmentConfirmed) services.Add("BOMBEIROS");

        return new OccurrenceServiceConfirmationReadDto
        {
            ConfirmedServices = services,
            ConfirmedBy = new OccurrenceCreatorDto
            {
                Id = occurrence.ServicesConfirmedByUser.Id,
                Username = occurrence.ServicesConfirmedByUser.Username,
                FullName = occurrence.ServicesConfirmedByUser.FullName
            },
            ConfirmedAt = occurrence.ServicesConfirmedAt.Value
        };
    }
}
