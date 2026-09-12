namespace Api.Dtos;

public class UserCreateDto
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FullName { get; set; }
    public required List<int> PermissionIds { get; set; } = new();
    public int? UnitId { get; set; }
    public int? HospitalId { get; set; }
}
