using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Security.Jwt;

public class JwtAuthentication
{
    private readonly RequestDelegate _next;

    public JwtAuthentication(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = JwtRequestTokenResolver.Resolve(context);
        if (!string.IsNullOrWhiteSpace(token))
        {
            try
            {
                context.User = JwtServices.Validate(token);
            }
            catch { context.User = new ClaimsPrincipal(new ClaimsIdentity()); }
        }

        await _next(context);
    }
}

public static class JwtAuthenticationExtensions
{
    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<JwtAuthentication>();
    }
}
