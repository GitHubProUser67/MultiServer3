using HTTPServer;
using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;

public static class HTTPServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static bool PHPDebugErrors { get; set; } = false;
    public static int HTTPPort { get; set; } = 80;
    public static int DefaultPluginsPort { get; set; } = 61850;
    public static int BufferSize = 4096;
    public static string HttpVersion = "1.1";
    public static string PluginParams { get; set; } = string.Empty;
    public static string HTTPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HomeToolsHelperStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/HomeToolsXMLs";
    public static bool EnableDiscordPlugin { get; set; } = true;
    public static string DiscordBotToken { get; set; } = string.Empty;
    public static string DiscordChannelID { get; set; } = string.Empty;
    public static List<string>? BannedIPs { get; set; }
    public static List<string>? AllowedIPs { get; set; }

    public static List<HTTPServer.PluginManager.IPlugin> plugins = HTTPServer.PluginManager.PluginLoader.LoadPluginsFromFolder(PluginsFolder);

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
            LoggerAccessor.LogWarn("Could not find the http.json file, using server's default.");
            return;
        }

        try
        {
            // Read the file
            string json = File.ReadAllText(configPath);

            // Parse the JSON configuration
            dynamic config = JObject.Parse(json);

            PHPRedirectUrl = config.php.redirct_url;
            PHPVersion = config.php.version;
            PHPStaticFolder = config.php.static_folder;
            PHPDebugErrors = config.php.debug_errors;
            HTTPPort = config.http_port;
            HTTPStaticFolder = config.http_static_folder;
            HomeToolsHelperStaticFolder = config.hometools_helper_static_folder;
            DefaultPluginsPort = config.default_plugins_port;
            BufferSize = config.buffer_size;
            HttpVersion = config.http_version;
            PluginParams = config.plugin_params;
            PluginsFolder = config.plugins_folder;
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
            LoggerAccessor.LogWarn("http.json file is malformed, using server's default.");
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

            HTTPServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/http.json");
        }
    }

    static void Main()
    {
        if (!BackendProject.MiscUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("HTTPServer");

        HTTPServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/http.json");

        var route_config = HTTPServer.RouteHandlers.staticRoutes.Main.index;

        if (HTTPServerConfiguration.EnableDiscordPlugin && !string.IsNullOrEmpty(HTTPServerConfiguration.DiscordChannelID) && !string.IsNullOrEmpty(HTTPServerConfiguration.DiscordBotToken))
            _ = BackendProject.Discord.CrudDiscordBot.BotStarter(HTTPServerConfiguration.DiscordChannelID, HTTPServerConfiguration.DiscordBotToken);

        _ = Task.Run(() => Parallel.Invoke(
                    () => new HttpServer(new int[] { 80, 3074, 9090, 10010, 33000 }, route_config, new CancellationTokenSource().Token),
                    () => RefreshConfig()
                ));

        if (HTTPServerConfiguration.plugins.Count > 0)
        {
            foreach (var plugin in HTTPServerConfiguration.plugins)
            {
                _ = plugin.HTTPStartPlugin("MultiServer", HTTPServerConfiguration.DefaultPluginsPort);
            }
        }

        if (BackendProject.MiscUtils.IsWindows())
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