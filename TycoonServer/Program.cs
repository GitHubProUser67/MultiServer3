using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;
using TycoonServer;

public static class TycoonServerConfiguration
{
    public static string TycoonStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
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
            LoggerAccessor.LogWarn("Could not find the tycoon.json file, using server's default.");
            return;
        }

        try
        {
            // Read the file
            string json = File.ReadAllText(configPath);

            // Parse the JSON configuration
            dynamic config = JObject.Parse(json);

            TycoonStaticFolder = config.tycoon_static_folder;
            JArray bannedIPsArray = config.BannedIPs;
            // Deserialize BannedIPs if it exists
            if (bannedIPsArray != null)
                BannedIPs = bannedIPsArray.ToObject<List<string>>();
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("tycoon.json file is malformed, using server's default.");
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

            TycoonServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/tycoon.json");
        }
    }

    static void Main()
    {
        if (!BackendProject.MiscUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("TycoonServer");

        TycoonServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/tycoon.json");

        Processor server = new("*", 61900);

        _ = Task.Run(() => Parallel.Invoke(
                    () => server.StartServer(),
                    () => RefreshConfig()
                ));

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