using System.Security.Claims;
using Api.Models;

namespace Api.Security.Jwt;

public static class JwtClaimsFactory
{
    public static IEnumerable<Claim> FromUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Identidade principal
        yield return new Claim("id", user.Id.ToString());
        yield return new Claim(ClaimTypes.Name, user.Username);

        // Claims informativos
        yield return new Claim("username", user.Username);
        yield return new Claim("fullName", user.FullName);
        yield return new Claim("email", user.Email);

        if (user.Unit is not null)
        {
            yield return new Claim("unitId", user.Unit.Id.ToString());
            yield return new Claim("unitName", user.Unit.Name);
            yield return new Claim("unitService", user.Unit.EmergencyService.Type.ToString());
        }
        if (user.Hospital is not null)
        {
            yield return new Claim("hospitalId", user.Hospital.Id.ToString());
            yield return new Claim("hospitalName", user.Hospital.Name);
        }

        if (user.AccessPermissions == null)
            yield break;

        foreach (var permission in user.AccessPermissions)
        {
            if (permission.SystemResource == null)
                continue;

            yield return new Claim(
                "permission",
                permission.SystemResource.Id.ToString()
            );
        }
    }
}
