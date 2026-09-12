using System.Net;
using System.Text.Json;
using Api.Auditing;
using Api.Data;
using Api.Dtos;
using Api.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace Api.Services.Occurrences;

public sealed class GetOccurrenceTimeline
{
    private readonly ApiDbContext _context;
    public GetOccurrenceTimeline(ApiDbContext context) => _context = context;

    public async Task<IReadOnlyList<OccurrenceTimelineItemDto>> ExecuteAsync(
        int occurrenceId,
        CancellationToken cancellationToken = default)
    {
        Guard.AgainstNonPositiveInt(occurrenceId);
        if (!await _context.Occurrences.AsNoTracking().AnyAsync(item => item.Id == occurrenceId, cancellationToken))
            throw new AppException("Ocorrência não encontrada.", (int)HttpStatusCode.NotFound);

        var actions = new Dictionary<string, string>
        {
            [SystemLogActionFactory.Create("Occurrence", occurrenceId)] = "Ocorrência registrada",
            [SystemLogActionFactory.Update("OccurrenceServiceConfirmation", occurrenceId)] = "Órgãos confirmados pelo atendente",
            [$"confirm Dispatch occurrence id: {occurrenceId}"] = "Despacho confirmado pelo atendente",
            [SystemLogActionFactory.Update("OccurrenceStatus", occurrenceId)] = "Status da ocorrência alterado"
        };
        var logs = await _context.SystemLogs.AsNoTracking()
            .Where(log => actions.Keys.Contains(log.Action))
            .OrderBy(log => log.CreatedAt).ThenBy(log => log.Id)
            .ToListAsync(cancellationToken);

        return logs.Select(log =>
        {
            var details = ReadTransition(log.Data);
            return new OccurrenceTimelineItemDto
            {
                Id = log.Id,
                Event = actions[log.Action],
                PreviousStatus = details.PreviousStatus,
                CurrentStatus = details.CurrentStatus,
                Reason = details.Reason,
                GeneratedBy = log.GeneratedBy ?? "Sistema",
                CreatedAt = log.CreatedAt
            };
        }).ToList();
    }

    private static (string? PreviousStatus, string? CurrentStatus, string? Reason) ReadTransition(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return (null, null, null);
        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            var previous = ReadString(root, "prevState", "status");
            var current = ReadString(root, "currState", "status") ?? ReadString(root, "created", "occurrenceStatus");
            var reason = ReadString(root, "currState", "reason");
            return (previous, current, reason);
        }
        catch (JsonException)
        {
            return (null, null, null);
        }
    }

    private static string? ReadString(JsonElement root, string objectName, string propertyName)
        => root.TryGetProperty(objectName, out var value)
           && value.ValueKind == JsonValueKind.Object
           && value.TryGetProperty(propertyName, out var property)
           && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
}
