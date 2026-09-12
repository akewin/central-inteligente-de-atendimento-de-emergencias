using Api.Dtos;
using Api.Models;

namespace Api.Services.Hospitals;

public static class HospitalMapper
{
    public static HospitalDto ToDto(Hospital h) => new()
    {
        Id = h.Id, Name = h.Name, PostalCode = h.PostalCode, Street = h.Street, Number = h.Number,
        Complement = h.Complement, Neighborhood = h.Neighborhood, City = h.City, State = h.State,
        Latitude = h.Latitude, Longitude = h.Longitude, HasEmergencyDepartment = h.HasEmergencyDepartment,
        Active = h.Active, UpdatedAt = h.UpdatedAt, Wards = h.Wards.OrderBy(w => w.Name).Select(ToDto).ToList()
    };
    public static HospitalWardDto ToDto(HospitalWard w) => new()
    {
        Id = w.Id, HospitalId = w.HospitalId, Name = w.Name, TotalBeds = w.TotalBeds,
        OccupiedBeds = w.OccupiedBeds, ReservedBeds = w.ReservedBeds, AvailableBeds = w.AvailableBeds,
        Active = w.Active, UpdatedAt = w.UpdatedAt
    };
    public static string Address(Hospital h) => $"{h.Street}, {h.Number} - {h.Neighborhood} - Araras/SP";
    public static PatientTransportDto ToDto(PatientTransport t) => new()
    {
        Id = t.Id, OccurrenceId = t.OccurrenceId, RequestedByDispatchId = t.RequestedByDispatchId,
        SamuDispatchId = t.SamuDispatchId, SamuUnitId = t.SamuDispatch?.UnitId, HospitalId = t.HospitalId, HospitalWardId = t.HospitalWardId,
        HospitalName = t.Hospital?.Name, WardName = t.HospitalWard?.Name, Status = t.Status.ToString(),
        OperationalNotes = t.OperationalNotes, Priority = t.Occurrence.ConfirmedPriority?.ToString(),
        Type = t.Occurrence.ConfirmedType?.ToString(), Description = t.Occurrence.Description,
        Origin = t.Occurrence.LocationDescription, EstimatedMinutes = t.EstimatedMinutes, AcknowledgedAt = t.AcknowledgedAt,
        CreatedAt = t.CreatedAt, UpdatedAt = t.UpdatedAt
    };
}
