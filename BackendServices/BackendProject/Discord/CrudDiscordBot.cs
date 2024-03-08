namespace BackendProject.Discord
{
    public static class CrudDiscordBot
    {
        private static string staticchannelid = string.Empty;

        private static DiscordBot? mybot = null;

        public static async Task BotStarter(string channelid, string token)
        {
            staticchannelid = channelid;
            mybot = new DiscordBot();
            if (mybot != null)
            {
                await mybot.RunBotAsync(token);
                CustomLogger.LoggerAccessor.LogInfo("[CrudDiscordBot] - Bot Subsystem initiated...");
            }
        }

        public static async Task BotSendMessage(string message)
        {
            ulong channelid = 0;
            if (mybot != null && !string.IsNullOrEmpty(staticchannelid) && ulong.TryParse(staticchannelid, out channelid))
                await mybot.SendMessageAsync(channelid, message);
        }
    }
}
