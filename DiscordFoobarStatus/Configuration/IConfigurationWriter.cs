namespace DiscordFoobarStatus.Configuration
{
    public interface IConfigurationWriter
    {
        public void Update<TOptions>(TOptions options, string sectionName)
            where TOptions : class;

        public void Reset(string sectionName);
    }
}
