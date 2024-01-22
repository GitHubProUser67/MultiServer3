using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;
using BackendProject;

public static class SRVEmuServerConfiguration
{
    public static string DirtySocksDatabaseConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/ea.dirtysocks.db.json";
    public static string ListenAddress { get; set; } = MiscUtils.GetLocalIPAddress().ToString();
    public static string GameServerAddress { get; set; } = "gameserver1.ps3.arcadia";
    public static int GameServerPort { get; set; } = 1003;
    public static string TheaterAddress { get; set; } = "theater.ps3.arcadia";
    public static bool EnableDiscordPlugin { get; set; } = true;
    public static string DiscordBotToken { get; set; } = string.Empty;
    public static string DiscordChannelID { get; set; } = string.Empty;

    /// <summary>
    /// Tries to load the specified configuration file.
    /// Throws an exception if it fails to find the file.
    /// </summary>
    /// <param name="configPath"></param>
    /// <exception cref="FileNotFoundException"></exception>
    public static void RefreshVariables(string configPath)
    {
        // Make sure the file exists
        if (!File.Exists(configPath))
        {
            LoggerAccessor.LogWarn("Could not find the srvemu.json file, using server's default.");
            return;
        }

        try
        {
            // Read the file
            string json = File.ReadAllText(configPath);

            // Parse the JSON configuration
            dynamic config = JObject.Parse(json);

            DirtySocksDatabaseConfig = config.database;
            ListenAddress = config.listen_address;
            GameServerAddress = config.game_server_address;
            GameServerPort = config.game_server_port;
            TheaterAddress = config.theater_address;
            DiscordBotToken = config.discord_bot_token;
            DiscordChannelID = config.discord_channel_id;
            EnableDiscordPlugin = config.discord_plugin.enabled;
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("srvemu.json file is malformed, using server's default.");
        }
    }
}

class Program
{
    static Task RefreshConfig()
    {
        while (true)
        {
            // Sleep for 5 minutes (300,000 milliseconds)
            Thread.Sleep(5 * 60 * 1000);

            // Your task logic goes here
            LoggerAccessor.LogInfo("Config Refresh at - " + DateTime.Now);

            SRVEmuServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/srvemu.json");
        }
    }

    static void Main()
    {
        if (!MiscUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("SRVEmu");

        SRVEmuServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/srvemu.json");

        if (SRVEmuServerConfiguration.EnableDiscordPlugin && !string.IsNullOrEmpty(SRVEmuServerConfiguration.DiscordChannelID) && !string.IsNullOrEmpty(SRVEmuServerConfiguration.DiscordBotToken))
            _ = BackendProject.Discord.CrudDiscordBot.BotStarter(SRVEmuServerConfiguration.DiscordChannelID, SRVEmuServerConfiguration.DiscordBotToken);

        SRVEmu.Arcadia.Storage.SharedCounters counters = new();
        SRVEmu.Arcadia.Storage.SharedCache caches = new();

        _ = Task.Run(() => Parallel.Invoke(
                    () => new SRVEmu.DirtySocks.DirtySocksServer().Run(),
                    () => new SRVEmu.Arcadia.Hosting.FeslHostedService(counters, caches).StartAsync(new CancellationTokenSource().Token),
                    () => new SRVEmu.Arcadia.Hosting.TheaterHostedService(counters, caches).StartAsync(new CancellationTokenSource().Token),
                    () => RefreshConfig()
                ));

        if (MiscUtils.IsWindows())
        {
            while (true)
            {
                LoggerAccessor.LogInfo("Press any key to shutdown the server. . .");

                Console.ReadLine();

                LoggerAccessor.LogWarn("Are you sure you want to shut down the server? [y/N]");
                char input = char.ToLower(Console.ReadKey().KeyChar);

                if (input == 'y')
                {
                    LoggerAccessor.LogInfo("Shutting down. Goodbye!");
                    Environment.Exit(0);
                }
            }
        }
        else
        {
            LoggerAccessor.LogWarn("\nConsole Inputs are locked while server is running. . .");

            Thread.Sleep(Timeout.Infinite); // While-true on Linux are thread blocking if on main static.
        }
    }
}