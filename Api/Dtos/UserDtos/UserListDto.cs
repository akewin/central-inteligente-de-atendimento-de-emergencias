namespace Api.Dtos;

public class UserListDto
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

