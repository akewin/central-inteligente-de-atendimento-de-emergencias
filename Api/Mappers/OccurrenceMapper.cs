using Api.Dtos;
using Api.Models;

namespace Api.Mappers;

public static class OccurrenceMapper
{
    public static OccurrenceReadDto ToReadDto(Occurrence occurrence)
    {
        return new OccurrenceReadDto
        {
            Id = occurrence.Id,
            Description = occurrence.Description,
            LocationDescription = occurrence.LocationDescription,
            PostalCode = occurrence.PostalCode,
            Street = occurrence.Street,
            Number = occurrence.Number,
            Complement = occurrence.Complement,
            Neighborhood = occurrence.Neighborhood,
            City = occurrence.City,
            State = occurrence.State,
            Reference = occurrence.AddressReference,
            Status = occurrence.Status.ToString(),
            ConfirmedType = occurrence.ConfirmedType?.ToString(),
            ConfirmedPriority = occurrence.ConfirmedPriority?.ToString(),
            CreatedBy = new OccurrenceCreatorDto
            {
                Id = occurrence.CreatedByUser.Id,
                Username = occurrence.CreatedByUser.Username,
                FullName = occurrence.CreatedByUser.FullName
            },
            CreatedAt = occurrence.CreatedAt,
            UpdatedAt = occurrence.UpdatedAt,
            LatestAIAnalysis = occurrence.AIAnalyses
                .OrderByDescending(a => a.CreatedAt)
                .ThenByDescending(a => a.Id)
                .Select(AIAnalysisMapper.ToReadDto)
                .FirstOrDefault(),
            ServiceConfirmation = OccurrenceServiceConfirmationMapper.ToReadDto(occurrence)
        };
    }
}
