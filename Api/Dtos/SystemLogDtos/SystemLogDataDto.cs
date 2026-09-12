namespace Api.Dtos;

public class SystemLogDataDto
{
    public string Type { get; set; } = string.Empty;
    public object? Created { get; set; }
    public object? PrevState { get; set; }
    public object? CurrState { get; set; }
}
