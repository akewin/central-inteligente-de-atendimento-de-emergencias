namespace Api.Security.Jwt;

public static class JwtRequestTokenResolver
{
    public static string? Resolve(HttpContext context)
    {
        var header = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(header) && header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return header["Bearer ".Length..].Trim();
        if (context.Request.Path.StartsWithSegments("/hubs/dispatches") &&
            context.Request.Query.TryGetValue("access_token", out var token) && !string.IsNullOrWhiteSpace(token))
            return token.ToString();
        return null;
    }
}
