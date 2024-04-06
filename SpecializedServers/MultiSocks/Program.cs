using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;
using BackendProject.MiscUtils;

public static class MultiSocksServerConfiguration
{
    public static string ServerBindAddress { get; set; } = VariousUtils.GetLocalIPAddress().ToString();
    public static string DirtySocksDatabaseConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/dirtysocks.db.json";
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
            LoggerAccessor.LogWarn("Could not find the MultiSocks.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath));

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("server_bind_address", ServerBindAddress),
                new JProperty("database", DirtySocksDatabaseConfig),
                new JProperty("discord_bot_token", DiscordBotToken),
                new JProperty("discord_channel_id", DiscordChannelID),
                new JProperty("discord_plugin", new JObject(
                    new JProperty("enabled", EnableDiscordPlugin)
                ))
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            ServerBindAddress = config.server_bind_address;
            DirtySocksDatabaseConfig = config.database;
            DiscordBotToken = config.discord_bot_token;
            DiscordChannelID = config.discord_channel_id;
            EnableDiscordPlugin = config.discord_plugin.enabled;
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("MultiSocks.json file is malformed, using server's default.");
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

            MultiSocksServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/MultiSocks.json");
        }
    }

    static void Main()
    {
        if (!VariousUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("MultiSocks");

        MultiSocksServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/MultiSocks.json");

        if (MultiSocksServerConfiguration.EnableDiscordPlugin && !string.IsNullOrEmpty(MultiSocksServerConfiguration.DiscordChannelID) && !string.IsNullOrEmpty(MultiSocksServerConfiguration.DiscordBotToken))
            _ = BackendProject.Discord.CrudDiscordBot.BotStarter(MultiSocksServerConfiguration.DiscordChannelID, MultiSocksServerConfiguration.DiscordBotToken);

        _ = Task.Run(() => Parallel.Invoke(
                    () => new MultiSocks.DirtySocks.DirtySocksServer().Run(),
                    () => RefreshConfig()
                ));

        if (VariousUtils.IsWindows())
        {
            while (true)
            {
                LoggerAccessor.LogInfo("Press any key to shutdown the server. . .");

                Console.ReadLine();

                LoggerAccessor.LogWarn("Are you sure you want to shut down the server? [y/N]");

                if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
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