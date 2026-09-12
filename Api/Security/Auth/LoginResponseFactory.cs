using Api.Dtos;
using Api.Models;
using Api.Security.Jwt;

namespace Api.Security.Auth;

public class LoginResponseFactory
{
    private readonly Api.Security.RefreshTokens.RefreshTokenServices _refreshService;

    public LoginResponseFactory(Api.Security.RefreshTokens.RefreshTokenServices refreshService)
    {
        _refreshService = refreshService;
    }

    public async Task<LoginResponseDto> CreateResponseAsync(User user)
    {
        var claims = JwtClaimsFactory.FromUser(user);
        var token = JwtServices.Create(claims);

        var (rawRefresh, _) = await _refreshService.GenerateAsync(user);

        var permissions = (user.AccessPermissions ?? [])
            .Where(ap => ap.SystemResource?.Active == true)
            .Select(ap => new SystemResourceReadDto
            {
                Id = ap.SystemResource!.Id,
                Name = ap.SystemResource.Name,
                ExhibitionName = ap.SystemResource.ExhibitionName
            })
            .ToList();

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = rawRefresh,
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            UnitId = user.UnitId,
            HospitalId = user.HospitalId,
            HospitalName = user.Hospital?.Name,
            Unit = user.Unit is null ? null : new OperationalUnitSummaryDto
            {
                Id = user.Unit.Id,
                Name = user.Unit.Name,
                Service = user.Unit.EmergencyService.Type.ToString()
            },
            Permissions = permissions
        };
    }
}
