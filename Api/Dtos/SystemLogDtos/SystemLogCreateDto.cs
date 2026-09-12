using Api.Dtos;

namespace Api.Dtos;

public class SystemLogCreateDto
{
    public string Action { get; set; } = string.Empty;
    public object? Data { get; set; }
}

