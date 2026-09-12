namespace Api.Dtos;

using System.Text.Json.Serialization;

public class UserUpdateDto
{
    public int Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? FullName { get; set; }
    public List<int>? PermissionIds { get; set; }
    private int? _unitId;
    public int? UnitId
    {
        get => _unitId;
        set { _unitId = value; HasUnitId = true; }
    }
    [JsonIgnore] public bool HasUnitId { get; private set; }
    private int? _hospitalId;
    public int? HospitalId
    {
        get => _hospitalId;
        set { _hospitalId = value; HasHospitalId = true; }
    }
    [JsonIgnore] public bool HasHospitalId { get; private set; }
}

