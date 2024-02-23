using BackendProject.MiscUtils;
using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;

public static class MitmDNSServerConfiguration
{
    public static string DNSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/routes.txt";
    public static string DNSOnlineConfig { get; set; } = string.Empty;
    public static bool DNSAllowUnsafeRequests { get; set; } = false;

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
            LoggerAccessor.LogWarn("Could not find the dns.json file, using server's default.");
            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            DNSOnlineConfig = config.online_routes_config;
            DNSConfig = config.routes_config;
            DNSAllowUnsafeRequests = config.allow_unsafe_requests;
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("dns.json file is malformed, using server's default.");
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

            MitmDNSServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/dns.json");
        }
    }

    static void Main()
    {
        if (!VariousUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("MitmDNS");

        MitmDNSServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/dns.json");

        MitmDNS.MitmDNSClass? dns = new();

        _ = Task.Run(() => Parallel.Invoke(
                    () => dns.MitmDNSMain(),
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