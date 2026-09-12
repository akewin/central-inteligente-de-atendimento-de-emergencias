using System.Security.Claims;
using Api.Security.Jwt;
using Microsoft.AspNetCore.Http;

namespace Api.Security.Jwt;

public class CurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;

    public int? GetId()
    {
        var claim = Principal?.FindFirst("id");
        return claim != null && int.TryParse(claim.Value, out var id)
            ? id
            : null;
    }

    public bool IsRoot()
        => Principal?.IsRoot() == true;

    public bool HasPermission(int permissionId)
        => Principal?.HasPermission(permissionId) == true;

    public int? GetUnitId() => Principal?.GetUnitId();
    public int? GetHospitalId() => Principal?.GetHospitalId();

    public string? GetUsername()
        => Principal?.FindFirst("username")?.Value
           ?? Principal?.FindFirst(ClaimTypes.Name)?.Value;

    public string? GetIpAddress()
        => _httpContextAccessor.HttpContext?
            .Connection?
            .RemoteIpAddress?
            .ToString();
}
