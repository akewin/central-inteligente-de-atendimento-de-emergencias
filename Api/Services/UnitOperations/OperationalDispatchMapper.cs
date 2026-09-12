using Api.Dtos;
using Api.Models;
using Api.Services.Hospitals;

namespace Api.Services.UnitOperations;

public static class OperationalDispatchMapper
{
    public static OperationalDispatchReadDto ToDto(Dispatch dispatch) => new()
    {
        Id = dispatch.Id,
        OccurrenceId = dispatch.OccurrenceId,
        Status = dispatch.Status.ToString(),
        CreatedAt = dispatch.CreatedAt,
        UpdatedAt = dispatch.UpdatedAt,
        AcceptedAt = dispatch.AcceptedAt,
        ArrivedAt = dispatch.ArrivedAt,
        ServiceStartedAt = dispatch.ServiceStartedAt,
        CompletedAt = dispatch.CompletedAt,
        CancelledAt = dispatch.CancelledAt,
        UnitName = dispatch.Unit.Name,
        UnitService = dispatch.Unit.EmergencyService.Type.ToString(),
        OccurrenceStatus = dispatch.Occurrence.Status.ToString(),
        Description = dispatch.Occurrence.Description,
        Address = FormatAddress(dispatch.Occurrence),
        Type = dispatch.Occurrence.ConfirmedType?.ToString(),
        Priority = dispatch.Occurrence.ConfirmedPriority?.ToString(),
        Services = GetServices(dispatch.Occurrence),
        Transport = dispatch.Occurrence.PatientTransports.OrderByDescending(t => t.Id).FirstOrDefault() is { } transport
            ? HospitalMapper.ToDto(transport) : null
    };

    private static string FormatAddress(Occurrence occurrence)
    {
        if (string.IsNullOrWhiteSpace(occurrence.Street)) return occurrence.LocationDescription;
        var cep = occurrence.PostalCode is { Length: 8 }
            ? $"CEP {occurrence.PostalCode[..5]}-{occurrence.PostalCode[5..]}" : occurrence.PostalCode;
        return string.Join(" · ", new[]
        {
            $"{occurrence.Street}, {occurrence.Number}", occurrence.Neighborhood,
            $"{occurrence.City} - {occurrence.State}", cep, occurrence.AddressReference
        }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static IReadOnlyList<string> GetServices(Occurrence occurrence)
    {
        var services = new List<string>(3);
        if (occurrence.PoliceConfirmed) services.Add("POLICIA");
        if (occurrence.SamuConfirmed) services.Add("SAMU");
        if (occurrence.FireDepartmentConfirmed) services.Add("BOMBEIROS");
        return services;
    }
}
