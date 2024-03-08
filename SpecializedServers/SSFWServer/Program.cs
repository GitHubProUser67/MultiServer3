using BackendProject.MiscUtils;
using CustomLogger;
using Newtonsoft.Json.Linq;
using SSFWServer;
using System.Runtime;

public static class SSFWServerConfiguration
{
    public static bool SSFWCrossSave { get; set; } = true;
    public static string SSFWMinibase { get; set; } = "[]";
    public static string SSFWLegacyKey { get; set; } = "**NoNoNoYouCantHaxThis****69";
    public static string SSFWStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwssfwroot";
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
    public static string ScenelistFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwssfwroot/SceneList.xml";
    public static bool EnableDiscordPlugin { get; set; } = true;
    public static string DiscordBotToken { get; set; } = string.Empty;
    public static string DiscordChannelID { get; set; } = string.Empty;
    public static List<string>? BannedIPs { get; set; }

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
            LoggerAccessor.LogWarn("Could not find the ssfw.json file, using server's default.");
            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            SSFWMinibase = config.minibase;
            SSFWLegacyKey = config.legacyKey;
            SSFWCrossSave = config.cross_save;
            SSFWStaticFolder = config.static_folder;
            HTTPSCertificateFile = config.certificate_file;
            ScenelistFile = config.scenelist_file;
            DiscordBotToken = config.discord_bot_token;
            DiscordChannelID = config.discord_channel_id;
            EnableDiscordPlugin = config.discord_plugin.enabled;
            JArray bannedIPsArray = config.BannedIPs;
            // Deserialize BannedIPs if it exists
            if (bannedIPsArray != null)
                BannedIPs = bannedIPsArray.ToObject<List<string>>();
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("ssfw.json file is malformed, using server's default.");
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

            SSFWServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/ssfw.json");
        }
    }

    static void Main()
    {
        if (!VariousUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("SSFWServer");

        SSFWServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/ssfw.json");

        SSLUtils.InitCerts(SSFWServerConfiguration.HTTPSCertificateFile);

        if (SSFWServerConfiguration.EnableDiscordPlugin && !string.IsNullOrEmpty(SSFWServerConfiguration.DiscordChannelID) && !string.IsNullOrEmpty(SSFWServerConfiguration.DiscordBotToken))
            _ = BackendProject.Discord.CrudDiscordBot.BotStarter(SSFWServerConfiguration.DiscordChannelID, SSFWServerConfiguration.DiscordBotToken);

        _ = new Timer(ScenelistParser.UpdateSceneDictionary, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

        _ = Task.Run(() => Parallel.Invoke(
                    () => new SSFWClass(SSFWServerConfiguration.HTTPSCertificateFile, "qwerty", SSFWServerConfiguration.SSFWLegacyKey).StartSSFW(),
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