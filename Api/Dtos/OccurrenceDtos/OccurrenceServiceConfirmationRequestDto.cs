using System.Text.Json.Serialization;

namespace Api.Dtos;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class OccurrenceServiceConfirmationRequestDto
{
    public string[]? ConfirmedServices { get; set; }
}
