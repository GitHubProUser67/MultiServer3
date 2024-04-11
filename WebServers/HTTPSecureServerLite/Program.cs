using CustomLogger;
using Newtonsoft.Json.Linq;
using HTTPSecureServerLite;
using System.Runtime;
using BackendProject.MiscUtils;
using HomeTools.AFS;
using WebUtils.LeaderboardsService;

public static class HTTPSServerConfiguration
{
    public static string DNSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/routes.txt";
    public static string DNSOnlineConfig { get; set; } = string.Empty;
    public static bool DNSAllowUnsafeRequests { get; set; } = true;
    public static string APIStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwapiroot";
    public static string HTTPSStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HTTPSTempFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwtemp";
    public static string ConvertersFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/converters";
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static bool PHPDebugErrors { get; set; } = false;
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
    public static string HomeToolsHelperStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/HomeToolsXMLs";
    public static bool UseSelfSignedCertificate { get; set; } = false;
    public static bool EnablePUTMethod { get; set; } = false;
    public static bool EnableDiscordPlugin { get; set; } = true;
    public static string DiscordBotToken { get; set; } = string.Empty;
    public static string DiscordChannelID { get; set; } = string.Empty;
    public static List<ushort>? Ports { get; set; } = new() { 443 };
    public static List<string>? RedirectRules { get; set; }
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
            LoggerAccessor.LogWarn("Could not find the https.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("online_routes_config", DNSOnlineConfig),
                new JProperty("routes_config", DNSConfig),
                new JProperty("allow_unsafe_requests", DNSAllowUnsafeRequests),
                new JProperty("api_static_folder", APIStaticFolder),
                new JProperty("php", new JObject(
                    new JProperty("redirect_url", PHPRedirectUrl),
                    new JProperty("version", PHPVersion),
                    new JProperty("static_folder", PHPStaticFolder),
                    new JProperty("debug_errors", PHPDebugErrors)
                )),
                new JProperty("https_static_folder", HTTPSStaticFolder),
                new JProperty("https_temp_folder", HTTPSTempFolder),
                new JProperty("converters_folder", ConvertersFolder),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("hometools_helper_static_folder", HomeToolsHelperStaticFolder),
                new JProperty("discord_bot_token", DiscordBotToken),
                new JProperty("discord_channel_id", DiscordChannelID),
                new JProperty("use_self_signed_certificate", UseSelfSignedCertificate),
                new JProperty("enable_put_method", EnablePUTMethod),
                new JProperty("discord_plugin", new JObject(
                    new JProperty("enabled", EnableDiscordPlugin)
                )),
                new JProperty("Ports", new JArray(Ports ?? new List<ushort> { })),
                new JProperty("RedirectRules", new JArray(RedirectRules ?? new List<string> { })),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { }))
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            DNSOnlineConfig = config.online_routes_config;
            DNSConfig = config.routes_config;
            DNSAllowUnsafeRequests = config.allow_unsafe_requests;
            APIStaticFolder = config.api_static_folder;
            PHPRedirectUrl = config.php.redirct_url;
            PHPVersion = config.php.version;
            PHPStaticFolder = config.php.static_folder;
            PHPDebugErrors = config.php.debug_errors;
            HTTPSStaticFolder = config.https_static_folder;
            HTTPSTempFolder = config.https_temp_folder;
            ConvertersFolder = config.converters_folder;
            HTTPSCertificateFile = config.certificate_file;
            HomeToolsHelperStaticFolder = config.hometools_helper_static_folder;
            DiscordBotToken = config.discord_bot_token;
            DiscordChannelID = config.discord_channel_id;
            UseSelfSignedCertificate = config.use_self_signed_certificate;
            EnablePUTMethod = config.enable_put_method;
            EnableDiscordPlugin = config.discord_plugin.enabled;
            JArray PortsArray = config.Ports;
            // Deserialize Ports if it exists
            if (PortsArray != null)
                Ports = PortsArray.ToObject<List<ushort>>();
            JArray redirectRulesArray = config.RedirectRules;
            // Deserialize RedirectRules if it exists
            if (redirectRulesArray != null)
                RedirectRules = redirectRulesArray.ToObject<List<string>>();
            JArray bannedIPsArray = config.BannedIPs;
            // Deserialize BannedIPs if it exists
            if (bannedIPsArray != null)
                BannedIPs = bannedIPsArray.ToObject<List<string>>();
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

        string certpath = HTTPSServerConfiguration.HTTPSCertificateFile;

        SSLUtils.InitCerts(certpath);

        GeoIPUtils.Initialize();

        AFSClass.MapperHelperFolder = HTTPSServerConfiguration.HomeToolsHelperStaticFolder;
        LeaderboardClass.APIPath = HTTPSServerConfiguration.APIStaticFolder;

        _ = new Timer(AFSClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));
        _ = new Timer(LeaderboardClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

        if (HTTPSServerConfiguration.EnableDiscordPlugin && !string.IsNullOrEmpty(HTTPSServerConfiguration.DiscordChannelID) && !string.IsNullOrEmpty(HTTPSServerConfiguration.DiscordBotToken))
            _ = BackendProject.Discord.CrudDiscordBot.BotStarter(HTTPSServerConfiguration.DiscordChannelID, HTTPSServerConfiguration.DiscordBotToken);

        _ = Task.Run(() => Parallel.Invoke(
                    () => SecureDNSConfigProcessor.InitDNSSubsystem(),
                    () => Parallel.ForEach(HTTPSServerConfiguration.Ports ?? new List<ushort> { }, port =>
                    {
                        new HttpsProcessor(HTTPSServerConfiguration.UseSelfSignedCertificate ? Path.GetDirectoryName(certpath) + $"/{Path.GetFileNameWithoutExtension(certpath)}_selfsigned.pfx" : certpath, "qwerty", "0.0.0.0", port).StartServer(); // 0.0.0.0 as the certificate binds to this IP
                    }),
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