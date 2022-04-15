using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DiscordFoobarStatus.Logging
{
    public static class FoobarLoggingBuilderExtensions
    {
        public static ILoggingBuilder AddFoobarConsole(this ILoggingBuilder builder)
        {
            builder.Services.TryAddSingleton<ILoggerProvider, FoobarConsoleLoggerProvider>();
            return builder;
        }
    }
}
