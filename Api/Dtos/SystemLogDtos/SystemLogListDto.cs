using Api.Dtos;

namespace Api.Dtos
{
    public class SystemLogListDto
    {
        public int Id { get; set; }
        public string GeneratedBy { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

    }
}
