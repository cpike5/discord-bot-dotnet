namespace DiscordBot.Core.Services
{
    public interface IBotControl
    {
        // Bot Lifecycle
        Task StartAsync();
        Task StopAsync();

        // Bot Status
        bool IsRunning { get; }
    }
}
