namespace DiscordFoobarStatus.Configuration
{
    public class ComponentOptions
    {
        public const string SectionName = "Component";

        public bool Disabled { get; set; }
        public long DiscordClientId { get; set; }
    }
}
