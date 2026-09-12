using Api.Auditing;
using Api.Auditing.Services;
using Api.Dtos;
using Api.Security.Jwt;
using Api.Security.Passwords;
using Api.Security.RefreshTokens;
using System.Security.Claims;

namespace Api.Security.Auth;

public class AuthServices
{
    private readonly AuthUserResolver _resolver;
    private readonly LoginResponseFactory _loginResponseFactory;
    private readonly RefreshTokenServices _refreshService;
    private readonly CreateSystemLog _log;

    public AuthServices(
        AuthUserResolver resolver,
        LoginResponseFactory loginResponseFactory,
        RefreshTokenServices refreshService,
        CreateSystemLog log)
    {
        _resolver = resolver;
        _loginResponseFactory = loginResponseFactory;
        _refreshService = refreshService;
        _log = log;
    }

    public async Task<LoginResponseDto?> LoginAsync(string identifier, string password)
    {
        var user = await _resolver.FindByIdentifierAsync(identifier);

        if (user == null || !PasswordHash.Verify(password, user.Password))
            return null;

        var response = await _loginResponseFactory.CreateResponseAsync(user);

        await _log.ExecuteAsync(
            SystemLogActionFactory.Login(user.Username),
            user.Id,
            user.Username
        );

        return response;
    }

    public async Task<LoginResponseDto?> LoginWithExternalTokenAsync(string externalToken)
    {
        ClaimsPrincipal principal;

        try
        {
            principal = JwtServices.Validate(externalToken);
        }
        catch
        {
            return null;
        }

        var user = await _resolver.FindByExternalClaimsAsync(principal);
        if (user == null)
            return null;

        var response = await _loginResponseFactory.CreateResponseAsync(user);

        await _log.ExecuteAsync(
            SystemLogActionFactory.ExternalLogin(user.Username),
            user.Id,
            user.Username
        );

        return response;
    }

    public async Task LogoutAsync(int userId, string username)
    {
        // revoke any outstanding refresh tokens
        await _refreshService.RevokeAllForUserAsync(userId);

        await _log.ExecuteAsync(
            SystemLogActionFactory.Logout(username),
            userId,
            username
        );
    }

    public async Task<RefreshTokenResponseDto?> RefreshAsync(string rawToken)
    {
        var stored = await _refreshService.ValidateAsync(rawToken);
        if (stored == null)
            return null;

        var user = stored.User!;
        var loginResponse = await _loginResponseFactory.CreateResponseAsync(user);

        // revoke the used token (rotation could be added here)
        await _refreshService.RevokeAsync(stored);

        await _log.ExecuteAsync(
            SystemLogActionFactory.TokenRefreshed(user.Username),
            user.Id,
            user.Username
        );

        return new RefreshTokenResponseDto
        {
            Token = loginResponse.Token,
            RefreshToken = loginResponse.RefreshToken
        };
    }
}
