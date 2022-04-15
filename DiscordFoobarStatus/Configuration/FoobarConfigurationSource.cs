using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Qwr.ComponentInterface;

namespace DiscordFoobarStatus.Configuration
{
    public class FoobarConfigurationSource : JsonStreamConfigurationSource, IConfigurationWriter
    {
        public event EventHandler<EventArgs>? Updated;

        private readonly IConfigVar<string> _configVar;

        private object _lock = new();

        public FoobarConfigurationSource(IConfigVar<string> configVar)
        {
            _configVar = configVar;
            Stream = new MemoryStream(Encoding.UTF8.GetBytes(_configVar.Get()));
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new FoobarConfigurationProvider(this);
        }

        public void Update<TOptions>(TOptions options, string sectionName)
            where TOptions : class
        {
            Update(x => x[sectionName] = JObject.FromObject(options));
        }

        public void Reset(string sectionName) => Update(x => x.Remove(sectionName));

        private void Update(Action<JObject> action)
        {
            lock (_lock)
            {
                var json = JObject.Parse(_configVar.Get());
                action(json);

                var serialized = JsonConvert.SerializeObject(json);
                _configVar.Set(serialized);
                Stream = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            }

            Updated?.Invoke(this, new EventArgs());
        }

        public static IConfigVar<string> Register(IStaticServicesManager manager)
        {
            return manager.RegisterConfigVar(new Guid("C37D0AA4-18A9-418B-819C-F7371DFE0440"), "{}");
        }
    }
}
