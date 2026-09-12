namespace Api.Dtos;

public sealed class OperationalUnitSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
}
