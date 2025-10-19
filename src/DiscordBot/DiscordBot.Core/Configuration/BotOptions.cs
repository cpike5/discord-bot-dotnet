namespace DiscordBot.Core.Configuration
{
    public class BotOptions
    {
        public static readonly string SectionName = "Bot";

        public string Token { get; set; } = string.Empty;
    }
}
