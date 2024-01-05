using CustomLogger;
using Discord;
using Discord.WebSocket;

namespace BackendProject.Discord
{
    public class DiscordBot
    {
        private DiscordSocketClient? _client;

        public async Task RunBotAsync(string token)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.DirectMessages | GatewayIntents.MessageContent
            });

            // Setup your client events
            _client.Log += Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public Task RegisterCommandsAsync()
        {
            if (_client != null)
                _client.MessageReceived += HandleCommandAsync;

            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            await Task.Run(() =>
            {
                // Activity is not from a Bot.
                if (!arg.Author.IsBot)
                {
                    ulong authorId = arg.Author.Id;
                    string channelId = arg.Channel.Id.ToString();
                    string messageId = arg.Id.ToString();
                    string message = arg.Content;

                    LoggerAccessor.LogInfo($"[Discord Bot] - Command Received - Author: {arg.Author} - Message: {message}");

                    SocketChannel? channel = _client?.GetChannel(Convert.ToUInt64(channelId));
                    ISocketMessageChannel? socketChannel = (ISocketMessageChannel?)channel;

                    if (!string.IsNullOrEmpty(message))
                    {
                        if (message.StartsWith("!"))
                        {
                            switch (message[1..])
                            {
                                default:
                                    socketChannel?.SendMessageAsync("Sorry my friend, but I have no idea what you want me to do ^^.");
                                    break;
                            }
                        }
                        else if (message == "ChuckNorris" && File.Exists(Directory.GetCurrentDirectory() + "/static/chucknorris.mp4"))
                            _ = socketChannel?.SendFileAsync(Directory.GetCurrentDirectory() + "/static/chucknorris.mp4");
                        else if (message == "OverHeat" && File.Exists(Directory.GetCurrentDirectory() + "/static/overheat.mp4"))
                            _ = socketChannel?.SendFileAsync(Directory.GetCurrentDirectory() + "/static/overheat.mp4");
                    }
                }
            });
        }

        private Task Log(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Info:
                    LoggerAccessor.LogInfo($"[DiscordBot] - Event: {arg.Message} From: {arg.Source}");
                    break;
                case LogSeverity.Debug:
                    LoggerAccessor.LogDebug($"[DiscordBot] - Event: {arg.Message} From: {arg.Source}");
                    break;
                case LogSeverity.Verbose:
                    LoggerAccessor.LogDebug($"[DiscordBot] - Event: {arg.Message} From: {arg.Source}");
                    break;
                case LogSeverity.Warning:
                    LoggerAccessor.LogWarn($"[DiscordBot] - Event: {arg.Message} From: {arg.Source}");
                    break;
                case LogSeverity.Error:
                    LoggerAccessor.LogError($"[DiscordBot] - Event: {arg.Message} From: {arg.Source}");
                    break;
                case LogSeverity.Critical:
                    LoggerAccessor.LogError($"[DiscordBot] - Event: {arg.Message} From: {arg.Source}");
                    break;
                default:
                    LoggerAccessor.LogInfo($"[DiscordBot] - Event: {arg.Message} From: {arg.Source} with unknown Severity");
                    break;
            }

            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(ulong channelId, string message)
        {
            IMessageChannel? channel = _client?.GetChannel(channelId) as IMessageChannel;
            if (channel != null)
                await channel.SendMessageAsync(message);
        }
    }
}
