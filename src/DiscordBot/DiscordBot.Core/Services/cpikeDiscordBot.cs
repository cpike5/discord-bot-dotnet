using Discord;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Core.Services
{
    public class cpikeDiscordBot : IBotControl
    {
        private readonly ILogger<cpikeDiscordBot> _logger;
        private readonly IDiscordClient _discordClient;

        public cpikeDiscordBot(ILogger<cpikeDiscordBot> logger, IDiscordClient discordClient)
        {
            _logger = logger;
            _discordClient = discordClient;
        }

        public bool IsRunning => throw new NotImplementedException();

        public Task StartAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync()
        {
            throw new NotImplementedException();
        }
    }
}
