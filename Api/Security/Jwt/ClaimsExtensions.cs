using System.Security.Claims;
using Api.Security.Permissions;

namespace Api.Security.Jwt;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
        => int.Parse(principal.FindFirstValue("id")!);

    public static IEnumerable<int> GetPermissionIds(
        this ClaimsPrincipal principal
    )
        => principal.Claims
            .Where(c => c.Type == "permission")
            .Select(c => int.Parse(c.Value));

    public static bool HasPermission(
        this ClaimsPrincipal principal,
        int permissionId
    )
        => principal.GetPermissionIds().Contains(permissionId);

    public static bool IsRoot(this ClaimsPrincipal principal)
        => principal.HasPermission(BasePermissions.ROOT);

    public static int? GetUnitId(this ClaimsPrincipal principal)
        => int.TryParse(principal.FindFirstValue("unitId"), out var unitId) ? unitId : null;
    public static int? GetHospitalId(this ClaimsPrincipal principal)
        => int.TryParse(principal.FindFirstValue("hospitalId"), out var hospitalId) ? hospitalId : null;
}
