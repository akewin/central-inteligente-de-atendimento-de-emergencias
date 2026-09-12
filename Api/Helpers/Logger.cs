using Microsoft.Extensions.Logging;

namespace Api.Helpers;

public static class Logger
{
    public static ILogger LogToConsole(string category)
    {
        return LoggerFactory.Create(builder => builder.AddConsole())
                            .CreateLogger(category);
    }
}
