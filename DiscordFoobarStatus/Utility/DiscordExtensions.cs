using Microsoft.Extensions.Logging;

namespace DiscordFoobarStatus.Utility
{
    public static class DiscordExtensions
    {
        public static LogLevel ToMicrosoft(this Discord.LogLevel x) => x switch
        {
            Discord.LogLevel.Error => LogLevel.Error,
            Discord.LogLevel.Warn => LogLevel.Warning,
            Discord.LogLevel.Info => LogLevel.Information,
            Discord.LogLevel.Debug => LogLevel.Debug,
            _ => LogLevel.None
        };
    }
}
