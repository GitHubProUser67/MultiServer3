using BlazeCommon;
using CustomLogger;
using NetworkLibrary.TCP_IP;
using MultiSocks.Aries;
using MultiSocks.Blaze;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Runtime;

public static class MultiSocksServerConfiguration
{
    public static bool UsePublicIPAddress { get; set; } = false;
    public static bool RPCS3Workarounds { get; set; } = true;
    public static string ProtoSSLCertificateCachePath { get; set; } = $"{Directory.GetCurrentDirectory()}/SSL/ProtoSSL/Cache";
    public static string DirtySocksDatabasePath { get; set; } = $"{Directory.GetCurrentDirectory()}/static/dirtysocks.db.json";

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
            LoggerAccessor.LogWarn("Could not find the MultiSocks.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("use_public_ipaddress", UsePublicIPAddress),
                new JProperty("rpcs3_workarounds", RPCS3Workarounds),
                new JProperty("protossl_certificate_cache_path", ProtoSSLCertificateCachePath),
                new JProperty("dirtysocks_database_path", DirtySocksDatabasePath)
            ).ToString());

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            UsePublicIPAddress = GetValueOrDefault(config, "use_public_ipaddress", UsePublicIPAddress);
            RPCS3Workarounds = GetValueOrDefault(config, "rpcs3_workarounds", RPCS3Workarounds);
            ProtoSSLCertificateCachePath = GetValueOrDefault(config, "protossl_certificate_cache_path", ProtoSSLCertificateCachePath);
            DirtySocksDatabasePath = GetValueOrDefault(config, "dirtysocks_database_path", DirtySocksDatabasePath);
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"MultiSocks.json file is malformed (exception: {ex}), using server's default.");
        }
    }

    // Helper method to get a value or default value if not present
    public static T GetValueOrDefault<T>(dynamic obj, string propertyName, T defaultValue)
    {
        try
        {
            if (obj != null)
            {
                if (obj is JObject jObject)
                {
                    if (jObject.TryGetValue(propertyName, out JToken? value))
                    {
                        T? returnvalue = value.ToObject<T>();
                        if (returnvalue != null)
                            return returnvalue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogError($"[Program] - GetValueOrDefault thrown an exception: {ex}");
        }

        return defaultValue;
    }
}

class Program
{
    private static string configDir = Directory.GetCurrentDirectory() + "/static/";
    private static string configPath = configDir + "MultiSocks.json";
    private static AriesServer? DirtySocksServer;
    private static BlazeClass? BlazeDirtySocksServer;

    private static void StartOrUpdateServer()
    {
        Directory.CreateDirectory(MultiSocksServerConfiguration.ProtoSSLCertificateCachePath);

        DirtySocksServer?.Dispose();
        BlazeDirtySocksServer?.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        DirtySocksServer = new AriesServer(new CancellationTokenSource().Token);
        BlazeDirtySocksServer = new BlazeClass(new CancellationTokenSource().Token);
    }

    static void Main()
    {
        if (!NetworkLibrary.Extension.OtherExtensions.IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        LoggerAccessor.SetupLogger("MultiSocks", Directory.GetCurrentDirectory());

#if DEBUG
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            LoggerAccessor.LogError("[Program] - A FATAL ERROR OCCURED!");
            LoggerAccessor.LogError(args.ExceptionObject as Exception);
        };

        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            LoggerAccessor.LogError("[Program] - A task has thrown a Unobserved Exception!");
            LoggerAccessor.LogError(args.Exception);
            args.SetObserved();
        };

        IPUtils.GetIPInfos(IPUtils.GetLocalIPAddress().ToString(), IPUtils.GetLocalSubnet());
#endif

        MultiSocksServerConfiguration.RefreshVariables(configPath);

        StartOrUpdateServer();

        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            while (true)
            {
                LoggerAccessor.LogInfo("Press any keys to access server actions...");

                Console.ReadLine();

                LoggerAccessor.LogInfo("Press one of the following keys to trigger an action: [R (Reboot),S (Shutdown)]");

                switch (char.ToLower(Console.ReadKey().KeyChar))
                {
                    case 's':
                        LoggerAccessor.LogWarn("Are you sure you want to shut down the server? [y/N]");

                        if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                        {
                            LoggerAccessor.LogInfo("Shutting down. Goodbye!");
                            Environment.Exit(0);
                        }
                        break;
                    case 'r':
                        LoggerAccessor.LogWarn("Are you sure you want to reboot the server? [y/N]");

                        if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                        {
                            LoggerAccessor.LogInfo("Rebooting!");

                            MultiSocksServerConfiguration.RefreshVariables(configPath);

                            StartOrUpdateServer();
                        }
                        break;
                }
            }
        }
        else
        {
            LoggerAccessor.LogWarn("\nConsole Inputs are locked while server is running. . .");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}