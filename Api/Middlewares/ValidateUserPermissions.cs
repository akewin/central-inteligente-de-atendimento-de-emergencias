using System.Net;
using System.Text.Json;
using System.Security.Claims;
using Api.Dtos;
using Api.Security.Jwt;
using Api.Security.Permissions;
using Microsoft.AspNetCore.Http;

namespace Api.Middlewares;

public class ValidateUserPermissions
{
    private readonly RequestDelegate _next;

    public ValidateUserPermissions(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        var method = context.Request.Method.ToUpper();

        if (path.Contains("/auth/") || path.Contains("/password/") || path.Contains("/options"))
        {
            await _next(context);
            return;
        }

        var token = JwtRequestTokenResolver.Resolve(context);
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Header Authorization ausente ou inválido.");
            return;
        }

        var principal = JwtServices.Validate(token);

        if (principal == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token inválido.");
            return;
        }

        if (principal.IsRoot())
        {
            await _next(context);
            return;
        }

        var requiredPermissions = EndpointPermissions.GetRequiredPermissions(path, method);

        if (requiredPermissions.Any() &&
            !requiredPermissions.Any(principal.HasPermission))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Acesso negado.");
            return;
        }

        if (path.StartsWith("/users") && (method == "POST" || method == "PUT"))
        {
            var bodyPermissions = await GetPermissionsFromBodyAsync(context);

            if (bodyPermissions.Contains(BasePermissions.ROOT) ||
                bodyPermissions.Contains(BasePermissions.RESOURCES))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(
                    "Acesso negado: não é permitido atribuir permissões root ou resources."
                );
                return;
            }
        }

        await _next(context);
    }

    private static async Task<int[]> GetPermissionsFromBodyAsync(HttpContext context)
    {
        context.Request.EnableBuffering();

        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        try
        {
            using var json = JsonDocument.Parse(body);
            if (json.RootElement.TryGetProperty("permissions", out var perms) &&
                perms.ValueKind == JsonValueKind.Array)
            {
                return perms.EnumerateArray().Select(p => p.GetInt32()).ToArray();
            }
        }
        catch { }

        return Array.Empty<int>();
    }
}

public static class ValidateUserPermissionsExtensions
{
    public static IApplicationBuilder UseValidateUserPermissions(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ValidateUserPermissions>();
    }
}
