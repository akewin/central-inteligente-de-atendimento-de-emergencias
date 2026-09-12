using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models;

public class SystemLog
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? Data { get; set; }

    public string? GeneratedBy { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
