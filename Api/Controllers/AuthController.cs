using Api.Dtos;
using Api.Security.Auth;
using Api.Security.Jwt;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthServices _authServices;

    public AuthController(AuthServices authServices)
    {
        _authServices = authServices;
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authServices.LoginAsync(
                request.Identifier,
                request.Password
            );

            if (response == null)
                return Unauthorized(new { message = "Credenciais inválidas." });

            return Ok(response);
        }
        catch
        {
            return StatusCode(
                500,
                new { message = "Erro ao processar login." }
            );
        }
    }

    // POST api/auth/external
    [HttpPost("external")]
    public async Task<IActionResult> LoginWithExternalToken(
        [FromBody] ExternalTokenRequestDto request
    )
    {
        try
        {
            var response =
                await _authServices.LoginWithExternalTokenAsync(
                    request.ExternalToken
                );

            if (response == null)
                return Unauthorized(new
                {
                    message = "Não foi possível autenticar com o token externo."
                });

            return Ok(response);
        }
        catch
        {
            return StatusCode(
                500,
                new { message = "Erro ao processar autenticação externa." }
            );
        }
    }

    // POST api/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromServices] CurrentUserContext currentUser)
    {
        if (!currentUser.IsAuthenticated)
            return Unauthorized(new { message = "Usuário não autenticado." });

        var userId = currentUser.GetId();
        var username = currentUser.GetUsername();

        if (userId == null || username == null)
            return Unauthorized(new { message = "Usuário inválido." });

        await _authServices.LogoutAsync(
            userId.Value,
            username
        );

        return NoContent();
    }

    // POST api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _authServices.RefreshAsync(request.RefreshToken);
            if (response == null)
                return Unauthorized(new { message = "Refresh token inválido." });

            return Ok(response);
        }
        catch
        {
            return StatusCode(
                500,
                new { message = "Erro ao processar refresh token." }
            );
        }
    }
}
