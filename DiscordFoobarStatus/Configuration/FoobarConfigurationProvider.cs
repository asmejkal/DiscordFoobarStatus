using Microsoft.Extensions.Configuration.Json;

namespace DiscordFoobarStatus.Configuration
{
    public class FoobarConfigurationProvider : JsonStreamConfigurationProvider
    {
        public FoobarConfigurationProvider(FoobarConfigurationSource source) 
            : base(source)
        {
            source.Updated += HandleSourceUpdated;
        }

        private void HandleSourceUpdated(object? sender, System.EventArgs e)
        {
            Load(Source.Stream);
            OnReload();
        }
    }
}
