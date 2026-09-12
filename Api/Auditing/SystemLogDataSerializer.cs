using Api.Dtos;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Auditing;

public static class SystemLogDataSerializer
{
    public static string Serialize(object data)
    {
        return JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            }
        );
    }

    public static SystemLogDataDto? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<SystemLogDataDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
    }
}
