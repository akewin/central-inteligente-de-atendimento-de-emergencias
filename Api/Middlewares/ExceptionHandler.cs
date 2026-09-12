using System.Net;
using System.Text.Json;

namespace Api.Middlewares;

public class ExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(
        RequestDelegate next,
        ILogger<ExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex,
                "Erro de aplicação: {Message} | Path: {Path} | User: {User}",
                ex.Message,
                context.Request.Path,
                context.User?.Identity?.Name);

            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                error = ex.Message,
                status = ex.StatusCode
            });

            await context.Response.WriteAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro não tratado: {Path} | User: {User}",
                context.Request.Path,
                context.User?.Identity?.Name);

            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new
            {
                error = "Ocorreu um erro inesperado.",
                status = 500
            });

            await context.Response.WriteAsync(result);
        }
    }
}

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandler>();
    }
}
