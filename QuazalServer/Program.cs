using CustomLogger;
using Newtonsoft.Json.Linq;
using QuazalServer.QNetZ;
using System.Runtime;

public static class QuazalServerConfiguration
{
    public static string? ServerBindAddress { get; set; } = "0.0.0.0";
    public static int RDVServerPort { get; set; } = 30200;
    public static int BackendServiceServerPort { get; set; } = 21006;
    public static string? ServerFilesPath { get; set; } = $"{Directory.GetCurrentDirectory()}/static/quazal";
    public static string? AccessKey { get; set; } = "yh64s"; // TDU_PS2 access key - Driver San Fransisco: w6kAtr3T

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
            LoggerAccessor.LogWarn("Could not find the quazal.json file, using server's default.");
            return;
        }

        try
        {
            // Read the file
            string json = File.ReadAllText(configPath);

            // Parse the JSON configuration
            dynamic config = JObject.Parse(json);

            ServerBindAddress = config.server_bind_address;
            RDVServerPort = config.server_rdv_port;
            BackendServiceServerPort = config.backend_server_port;
            ServerFilesPath = config.server_files_path;
            AccessKey = config.access_key;
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("quazal.json file is malformed, using server's default.");
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

            QuazalServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/quazal.json");
        }
    }

    static void Main()
    {
        if (!CryptoSporidium.MiscUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("QuazalServer");

        QuazalServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/quazal.json");

        QCheckSum.Instance = new();
		
        QuazalServer.ServerProcessors.BackendServicesServer.Start();

        QuazalServer.ServerProcessors.RDVServer.Start();

        _ = Task.Run(RefreshConfig);

        if (CryptoSporidium.MiscUtils.IsWindows())
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