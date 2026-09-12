namespace Api.Dtos;

public class UnitCreateDto
{
    public string? Name { get; set; }
    public string? Service { get; set; }
    public string? PostalCode { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Reference { get; set; }
    public string? Status { get; set; }
}

public sealed class UnitUpdateDto : UnitCreateDto
{
    public DateTime? ExpectedUpdatedAt { get; set; }
}

public sealed class UnitReadDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EmergencyServiceReadDto Service { get; set; } = new();
    public string? PostalCode { get; set; }
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Complement { get; set; }
    public string? Neighborhood { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Reference { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
