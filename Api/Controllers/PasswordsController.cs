using Api.Dtos;
using Api.Helpers;
using Api.Middlewares;
using Api.Services.AuthServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Api.Controllers;

[ApiController]
[Route("api/password")]
public class PasswordController : ControllerBase
{
    private readonly PasswordServices _passwordServices;

    public PasswordController(PasswordServices passwordServices)
    {
        _passwordServices = passwordServices;
    }

    // POST api/password/request-new
    [HttpPost("request-new")]
    public async Task<IActionResult> RequestNewPassword(
        [FromBody] RequestNewPasswordDto request
    )
    {
        try
        {
            await _passwordServices.RequestNewPasswordAsync(request.Email);

            return Ok(new
            {
                message =
                    "Um link de redefinição de senha foi enviado para o e-mail informado."
            });
        }
        catch (AppException ex)
        {
            return StatusCode(
                ex.StatusCode,
                new { message = ex.Message }
            );
        }
        catch
        {
            return StatusCode(
                500,
                new { message = "Erro ao processar solicitação de redefinição de senha." }
            );
        }
    }

    // POST api/password/reset
    [HttpPost("reset")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordDto request
    )
    {
        try
        {
            await _passwordServices.ResetPasswordAsync(
                request.Token,
                request.NewPassword
            );

            return Ok(new
            {
                message = "Senha redefinida com sucesso."
            });
        }
        catch (AppException ex)
        {
            return StatusCode(
                ex.StatusCode,
                new { message = ex.Message }
            );
        }
        catch
        {
            return StatusCode(
                500,
                new { message = "Erro ao redefinir senha." }
            );
        }
    }
}
