using HTTPServer;
using CustomLogger;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

public static class HTTPServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/plugins";
    public static string PHPVersion { get; set; } = "php-8.2.9";
    public static string PHPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/PHP";
    public static bool PHPDebugErrors { get; set; } = false;
    public static int HTTPPort { get; set; } = 80;
    public static string PluginParams { get; set; } = string.Empty;
    public static string HTTPStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HomeToolsHelperStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/HomeToolsXMLs";
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
            LoggerAccessor.LogWarn("Could not find the http.json file, using server's default.");
            return;
        }

        // Read the file
        string json = File.ReadAllText(configPath);

        // Parse the JSON configuration
        dynamic config = JObject.Parse(json);

        PHPVersion = config.php.version;
        PHPStaticFolder = config.php.static_folder;
        PHPDebugErrors = config.php.debug_errors;
        HTTPPort = config.http_port;
        PluginParams = config.plugin_params;
        PluginsFolder = config.plugins_folder;
        JArray bannedIPsArray = config.BannedIPs;
        // Deserialize BannedIPs if it exists
        if (bannedIPsArray != null)
            BannedIPs = bannedIPsArray.ToObject<List<string>>();
        JArray allowedIPsArray = config.AllowedIPs;
        // Deserialize BannedIPs if it exists
        if (allowedIPsArray != null)
            AllowedIPs = allowedIPsArray.ToObject<List<string>>();
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
        if (Misc.IsWindows())
            if (!Misc.IsAdministrator())
            {
                Console.WriteLine("Trying to restart as admin");
                if (Misc.StartAsAdmin(Process.GetCurrentProcess().MainModule.FileName))
                    Environment.Exit(0);
            }

        LoggerAccessor.SetupLogger("HTTPServer");

        HTTPServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/http.json");

        Processor server = new("*", HTTPServerConfiguration.HTTPPort);

        server.Start();

        _ = Task.Run(RefreshConfig);

        if (Misc.IsWindows())
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