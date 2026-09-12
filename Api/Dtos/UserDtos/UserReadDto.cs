using System;
using System.Collections.Generic;

namespace Api.Dtos;

public class UserReadDto
{
    public int Id { get; set; }
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;

    public List<SystemResourceSelectDto> Permissions { get; set; } = new();
    public int? UnitId { get; set; }
    public OperationalUnitSummaryDto? Unit { get; set; }
    public int? HospitalId { get; set; }
    public string? HospitalName { get; set; }
}

