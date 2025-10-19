using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordBot(this IServiceCollection services, IConfiguration configuration)
        {
            // Register the Discord client as a singleton
            services.AddSingleton<IDiscordClient, DiscordSocketClient>(provider =>
            {
                var config = new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.All
                };
                return new DiscordSocketClient(config);
            });

            return services;
        }
    }
}
