using System.Collections.Generic;

namespace Api.Dtos
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public int Id { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public IEnumerable<SystemResourceReadDto> Permissions { get; set; } = [];
        public int? UnitId { get; set; }
        public OperationalUnitSummaryDto? Unit { get; set; }
        public int? HospitalId { get; set; }
        public string? HospitalName { get; set; }
    }
}
