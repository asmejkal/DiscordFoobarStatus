using Microsoft.Extensions.DependencyInjection;

namespace DiscordFoobarStatus.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClientServices(this IServiceCollection services)
        {
            services.AddTransient<IDiscordClient, DiscordClient>();
            services.AddTransient<Music>();

            return services;
        }
    }
}
