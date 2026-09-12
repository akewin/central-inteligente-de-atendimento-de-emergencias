using Api.Helpers;
using Api.Security.Jwt;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace Api.Middlewares
{
    public class RequireAuthorization
    {
        private readonly RequestDelegate _next;

        public RequireAuthorization(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";
            var method = context.Request.Method.ToUpper();

            if (path.Contains("/auth/") || path.Contains("/password/"))
            {
                await _next(context);
                return;
            }

            if (method is "GET" or "POST" or "PUT" or "DELETE")
            {
                var token = JwtRequestTokenResolver.Resolve(context);
                if (string.IsNullOrWhiteSpace(token))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Header Authorization ausente ou inválido");
                    return;
                }

                if (!JwtServices.IsValid(token))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Token inválido ou expirado");
                    return;
                }
            }

            await _next(context);
        }
    }

    // ✅ Extensão para facilitar o uso no Program.cs
    public static class RequireAuthorizationExtensions
    {
        public static IApplicationBuilder UseRequireAuthorization(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequireAuthorization>();
        }
    }
}
