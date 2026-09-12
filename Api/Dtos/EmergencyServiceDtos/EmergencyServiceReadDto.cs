namespace Api.Dtos;

public sealed class EmergencyServiceReadDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string EmergencyNumber { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
