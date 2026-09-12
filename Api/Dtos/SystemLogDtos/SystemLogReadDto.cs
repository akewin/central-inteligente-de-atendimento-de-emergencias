using Api.Dtos;

namespace Api.Dtos;

public class SystemLogReadDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public SystemLogDataDto? Data { get; set; }
    public string GeneratedBy { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

