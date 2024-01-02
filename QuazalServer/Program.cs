using BackendProject;
using CustomLogger;
using Newtonsoft.Json.Linq;
using System.Runtime;

public static class QuazalServerConfiguration
{
    public static string ServerBindAddress { get; set; } = MiscUtils.GetLocalIPAddress().ToString();
    public static string EdNetBindAddress { get; set; } = MiscUtils.GetLocalIPAddress().ToString();
    public static string QuazalStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/Quazal";

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
            EdNetBindAddress = config.ednet_bind_address;
            QuazalStaticFolder = config.server_static_folder;
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
        if (!MiscUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("QuazalServer");

        QuazalServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/quazal.json");

        QuazalServer.RDVServices.ServiceFactoryRDV.RegisterRDVServices();

        _ = Task.Run(() => Parallel.Invoke(
                    () => new QuazalServer.ServerProcessors.BackendServicesServer().Start(30201, 2, "yh64s"), // TDU
                    () => new QuazalServer.ServerProcessors.RDVServer().Start(30200, 30201, 2, "yh64s"),
                    () => new QuazalServer.ServerProcessors.BackendServicesServer().Start(60106, 2, "w6kAtr3T"), // DFSPC
                    () => new QuazalServer.ServerProcessors.RDVServer().Start(60105, 60106, 2, "w6kAtr3T"),
                    () => new QuazalServer.ServerProcessors.BackendServicesServer().Start(61111, 2, "QusaPha9"), // DFSPS3
                    () => new QuazalServer.ServerProcessors.RDVServer().Start(61110, 61111, 2, "QusaPha9"),
                    () => new QuazalServer.ServerProcessors.BackendServicesServer().Start(60001, 2, "ridfebb9"), // RB3
                    () => new QuazalServer.ServerProcessors.BackendServicesServer().Start(21032, 2, "8dtRv2oj"), // GRO
                    () => RefreshConfig()
                ));

        if (MiscUtils.IsWindows())
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