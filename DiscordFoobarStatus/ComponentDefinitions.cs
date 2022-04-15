using System.Reflection;

namespace DiscordFoobarStatus
{
    public static class ComponentDefinitions
    {
        public const string Name = "Discord Status (Rich Presence)";
        public const string ShortName = "DiscordFoobarStatus";
        public const string Description = "Shows a now playing status in your Discord profile";
        public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    }
}
