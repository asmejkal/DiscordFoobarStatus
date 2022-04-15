using System;
using System.IO;
using System.Reflection;
using DiscordFoobarStatus.Configuration;
using DiscordFoobarStatus.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qwr.ComponentInterface;

namespace DiscordFoobarStatus
{
    public class Component : IComponent
    {
        private static IServiceProvider? _services; // Need this singleton for use in objects created by the host (no locks required)
        public static IServiceProvider Services => _services ?? throw new InvalidOperationException("Not initialized");

        private IHost? _host;
        private IConfigVar<string>? _configVar;

        public ComponentInfo GetInfo()
        {
            return new()
            {
                Name = ComponentDefinitions.Name,
                Description = ComponentDefinitions.Description,
                Version = ComponentDefinitions.Version,
            };
        }

        [ComponentInterfaceVersion("0.1.1")]
        public Component()
        {
        }

        public void Initialize(IStaticServicesManager servicesManager, IUtils utils)
        {
            _configVar = FoobarConfigurationSource.Register(servicesManager);

            servicesManager.RegisterPreferencesPage(new PreferencesPageInfo()
            {
                Guid = new Guid("D79F0BFF-4326-44D2-8CBF-86F31169B8EE"),
                Name = ComponentDefinitions.Name,
                ParentGuid = utils.Fb2kGuid(Fb2kGuidId.PrefPage_Tools)
            }, typeof(PreferencesPage));
        }

        public void Shutdown()
        {
            _host?.StopAsync().Wait();
            _host?.Dispose();
        }

        public void Start(IDynamicServicesManager servicesManager, IControls controls)
        {
            var configurationSource = new FoobarConfigurationSource(_configVar ?? throw new InvalidOperationException("Not initialized"));

            _host = new HostBuilder()
                .ConfigureAppConfiguration(x => x.Add(configurationSource))
                .ConfigureServices(services =>
                {
                    services.AddOptions<FormattingOptions>().BindConfiguration(FormattingOptions.SectionName);
                    services.AddOptions<ComponentOptions>().BindConfiguration(ComponentOptions.SectionName);
                    services.AddOptions<FoobarConsoleLoggerOptions>().Configure(x => x.ComponentName = ComponentDefinitions.ShortName);

                    services.AddSingleton(controls);
                    services.AddSingleton(controls.Console());
                    services.AddSingleton(servicesManager.RegisterForPlaybackCallbacks());

                    services.AddSingleton<IConfigurationWriter>(configurationSource);
                    services.AddSingleton<IDiscordClient, DiscordClient>();
                    services.AddHostedService<DiscordStatusService>();
                    services.AddHttpClient<IDeezerClient, DeezerClient>(x => x.BaseAddress = new Uri("https://api.deezer.com/"));
                })
                .ConfigureLogging(x => x.AddFoobarConsole().SetMinimumLevel(LogLevel.Information).AddFilter("System.Net.Http", LogLevel.None))
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .Build();

            _services = _host.Services;
            _host.Start();
        }
    }
}