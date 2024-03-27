using HTTPServer;
using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;
using BackendProject.MiscUtils;
using HomeTools.AFS;

public static class HTTPServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static string PHPVersion { get; set; } = "php-8.3.0";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static string PHPRedirectUrl { get; set; } = string.Empty;
    public static bool PHPDebugErrors { get; set; } = false;
    public static ushort DefaultPluginsPort { get; set; } = 61850;
    public static int BufferSize { get; set; } = 4096;
    public static string HttpVersion { get; set; } = "1.1";
    public static string PluginParams { get; set; } = string.Empty;
    public static string APIStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwapiroot";
    public static string HTTPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HTTPTempFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwtemp";
    public static string ConvertersFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/converters";
    public static string HomeToolsHelperStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/HomeToolsXMLs";
    public static bool EnablePUTMethod { get; set; } = false;
    public static bool EnableImageUpscale { get; set; } = false;
    public static bool EnableDiscordPlugin { get; set; } = true;
    public static string DiscordBotToken { get; set; } = string.Empty;
    public static string DiscordChannelID { get; set; } = string.Empty;
    public static List<ushort>? Ports { get; set; } = new() { 80, 3074, 9090, 10010, 33000 };
    public static List<string>? RedirectRules { get; set; }
    public static List<string>? BannedIPs { get; set; }
    public static List<string>? AllowedIPs { get; set; }

    public static List<HTTPServer.PluginManager.HTTPPlugin> plugins = HTTPServer.PluginManager.PluginLoader.LoadPluginsFromFolder(PluginsFolder);

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
            LoggerAccessor.LogWarn("Could not find the http.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath));

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("php", new JObject(
                    new JProperty("redirect_url", PHPRedirectUrl),
                    new JProperty("version", PHPVersion),
                    new JProperty("static_folder", PHPStaticFolder),
                    new JProperty("debug_errors", PHPDebugErrors)
                )),
                new JProperty("api_static_folder", APIStaticFolder),
                new JProperty("http_static_folder", HTTPStaticFolder),
                new JProperty("http_temp_folder", HTTPTempFolder),
                new JProperty("converters_folder", ConvertersFolder),
                new JProperty("hometools_helper_static_folder", HomeToolsHelperStaticFolder),
                new JProperty("default_plugins_port", DefaultPluginsPort),
                new JProperty("buffer_size", BufferSize),
                new JProperty("http_version", HttpVersion),
                new JProperty("plugin_params", PluginParams),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("discord_bot_token", DiscordBotToken),
                new JProperty("discord_channel_id", DiscordChannelID),
                new JProperty("enable_put_method", EnablePUTMethod),
                new JProperty("enable_image_upscale", EnableImageUpscale),
                new JProperty("discord_plugin", new JObject(
                    new JProperty("enabled", EnableDiscordPlugin)
                )),
                new JProperty("Ports", new JArray(Ports ?? new List<ushort> { })),
                new JProperty("RedirectRules", new JArray(RedirectRules ?? new List<string> { })),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { })),
                new JProperty("AllowedIPs", new JArray(AllowedIPs ?? new List<string> { }))
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            PHPRedirectUrl = config.php.redirct_url;
            PHPVersion = config.php.version;
            PHPStaticFolder = config.php.static_folder;
            PHPDebugErrors = config.php.debug_errors;
            APIStaticFolder = config.api_static_folder;
            HTTPStaticFolder = config.http_static_folder;
            HTTPTempFolder = config.http_temp_folder;
            ConvertersFolder = config.converters_folder;
            HomeToolsHelperStaticFolder = config.hometools_helper_static_folder;
            DefaultPluginsPort = config.default_plugins_port;
            BufferSize = config.buffer_size;
            HttpVersion = config.http_version;
            PluginParams = config.plugin_params;
            PluginsFolder = config.plugins_folder;
            DiscordBotToken = config.discord_bot_token;
            DiscordChannelID = config.discord_channel_id;
            EnablePUTMethod = config.enable_put_method;
            EnableImageUpscale = config.enable_image_upscale;
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
        if (!VariousUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("HTTPServer");

        HTTPServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/http.json");

        GeoIPUtils.Initialize();

        AFSClass.MapperHelperFolder = HTTPServerConfiguration.HomeToolsHelperStaticFolder;

        _ = new Timer(AFSClass.ScheduledUpdate, null, TimeSpan.Zero, TimeSpan.FromMinutes(1440));

        if (HTTPServerConfiguration.EnableDiscordPlugin && !string.IsNullOrEmpty(HTTPServerConfiguration.DiscordChannelID) && !string.IsNullOrEmpty(HTTPServerConfiguration.DiscordBotToken))
            _ = BackendProject.Discord.CrudDiscordBot.BotStarter(HTTPServerConfiguration.DiscordChannelID, HTTPServerConfiguration.DiscordBotToken);

        _ = Task.Run(() => Parallel.Invoke(
                    () => _ = new HttpServer(HTTPServerConfiguration.Ports, HTTPServer.RouteHandlers.staticRoutes.Main.index, new CancellationTokenSource().Token),
                    () => RefreshConfig()
                ));

        if (HTTPServerConfiguration.plugins.Count > 0)
        {
            int i = 0;
            foreach (HTTPServer.PluginManager.HTTPPlugin plugin in HTTPServerConfiguration.plugins)
            {
                _ = plugin.HTTPStartPlugin("MultiServer", (ushort)(HTTPServerConfiguration.DefaultPluginsPort + i));
                i++;
            }
        }

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