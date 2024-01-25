using CustomLogger;
using Newtonsoft.Json.Linq;
using HTTPSecureServerLite;
using System.Runtime;
using BackendProject.MiscUtils;

public static class HTTPSServerConfiguration
{
    public static string DNSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/routes.txt";
    public static string DNSOnlineConfig { get; set; } = string.Empty;
    public static bool DNSAllowUnsafeRequests { get; set; } = false;
    public static string APIStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwapiroot";
    public static string HTTPSStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static bool PHPDebugErrors { get; set; } = false;
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
    public static string HomeToolsHelperStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/HomeToolsXMLs";
    public static bool EnableDiscordPlugin { get; set; } = true;
    public static string DiscordBotToken { get; set; } = string.Empty;
    public static string DiscordChannelID { get; set; } = string.Empty;
    public static List<string>? BannedIPs { get; set; }
    public static List<string>? AllowedIPs { get; set; }

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
            LoggerAccessor.LogWarn("Could not find the https.json file, using server's default.");
            return;
        }

        try
        {
            // Read the file
            string json = File.ReadAllText(configPath);

            // Parse the JSON configuration
            dynamic config = JObject.Parse(json);

            DNSOnlineConfig = config.online_routes_config;
            DNSConfig = config.routes_config;
            DNSAllowUnsafeRequests = config.allow_unsafe_requests;
            APIStaticFolder = config.api_static_folder;
            PHPRedirectUrl = config.php.redirct_url;
            PHPVersion = config.php.version;
            PHPStaticFolder = config.php.static_folder;
            PHPDebugErrors = config.php.debug_errors;
            HTTPSStaticFolder = config.https_static_folder;
            HTTPSCertificateFile = config.certificate_file;
            HomeToolsHelperStaticFolder = config.hometools_helper_static_folder;
            DiscordBotToken = config.discord_bot_token;
            DiscordChannelID = config.discord_channel_id;
            EnableDiscordPlugin = config.discord_plugin.enabled;
            JArray bannedIPsArray = config.BannedIPs;
            // Deserialize BannedIPs if it exists
            if (bannedIPsArray != null)
                BannedIPs = bannedIPsArray.ToObject<List<string>>();
            JArray allowedIPsArray = config.AllowedIPs;
            // Deserialize BannedIPs if it exists
            if (allowedIPsArray != null)
                AllowedIPs = allowedIPsArray.ToObject<List<string>>();
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("https.json file is malformed, using server's default.");
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

            HTTPSServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/https.json");
        }
    }

    static void Main()
    {
        if (!VariousUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("HTTPSecureServer");

        HTTPSServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/https.json");

        SSLUtils.InitCerts(HTTPSServerConfiguration.HTTPSCertificateFile);

        // Timer for scheduled updates every 24 hours
        _ = new Timer(HTTPSecureServerLite.API.VEEMEE.goalie_sfrgbt.ScoreBoardData.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        _ = new Timer(HTTPSecureServerLite.API.VEEMEE.gofish.ScoreBoardData.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        _ = new Timer(HTTPSecureServerLite.API.VEEMEE.olm.ScoreBoardData.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

        if (HTTPSServerConfiguration.EnableDiscordPlugin && !string.IsNullOrEmpty(HTTPSServerConfiguration.DiscordChannelID) && !string.IsNullOrEmpty(HTTPSServerConfiguration.DiscordBotToken))
            _ = BackendProject.Discord.CrudDiscordBot.BotStarter(HTTPSServerConfiguration.DiscordChannelID, HTTPSServerConfiguration.DiscordBotToken);

        _ = Task.Run(() => Parallel.Invoke(
                    () => new HttpsProcessor(HTTPSServerConfiguration.HTTPSCertificateFile, "qwerty", "0.0.0.0", 443).StartServer(), // 0.0.0.0 as the certificate binds to this ip.
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