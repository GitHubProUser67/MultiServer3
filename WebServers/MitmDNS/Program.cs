using HashLib;
using CustomLogger;
using NetworkLibrary.TCP_IP;
using MitmDNS;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

public static class MitmDNSServerConfiguration
{
    public static string DNSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/routes.txt";
    public static string DNSOnlineConfig { get; set; } = string.Empty;
    public static uint MaximumNumberOfRequests { get; set; } = 200;
    public static uint MinimumDeltaTimeMs { get; set; } = 1000;
    public static bool DNSAllowUnsafeRequests { get; set; } = true;

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
            LoggerAccessor.LogWarn("Could not find the dns.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("online_routes_config", DNSOnlineConfig),
                new JProperty("maximum_number_of_requests", MaximumNumberOfRequests),
                new JProperty("minimum_delta_time_ms", MinimumDeltaTimeMs),
                new JProperty("routes_config", DNSConfig),
                new JProperty("allow_unsafe_requests", DNSAllowUnsafeRequests)
            ).ToString());

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            DNSOnlineConfig = GetValueOrDefault(config, "online_routes_config", DNSOnlineConfig);
            MaximumNumberOfRequests = GetValueOrDefault(config, "maximum_number_of_requests", MaximumNumberOfRequests);
            MinimumDeltaTimeMs = GetValueOrDefault(config, "minimum_delta_time_ms", MinimumDeltaTimeMs);
            DNSConfig = GetValueOrDefault(config, "routes_config", DNSConfig);
            DNSAllowUnsafeRequests = GetValueOrDefault(config, "allow_unsafe_requests", DNSAllowUnsafeRequests);
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"dns.json file is malformed (exception: {ex}), using server's default.");
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
                    if (jObject.TryGetValue(propertyName, out JToken value))
                    {
                        T returnvalue = value.ToObject<T>();
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
    private static string configPath = configDir + "dns.json";
    private static string DNSconfigMD5 = string.Empty;
    private static Task DNSThread = null;
    private static Task DNSRefreshThread = null;
    private static MitmDNSClass Server = new MitmDNSClass();
    private static readonly FileSystemWatcher dnswatcher = new FileSystemWatcher();

    // Event handler for DNS change event
    private static void OnDNSChanged(object source, FileSystemEventArgs e)
    {
        try
        {
            dnswatcher.EnableRaisingEvents = false;

            LoggerAccessor.LogInfo($"DNS Routes File {e.FullPath} has been changed, Routes Refresh at - {DateTime.Now}");

            // Sleep a little to let file-system time to write the changes to the file.
            Thread.Sleep(6000);

            DNSconfigMD5 = ComputeMD5FromFile(MitmDNSServerConfiguration.DNSConfig);

            while (DNSRefreshThread != null)
            {
                LoggerAccessor.LogWarn("[DNS] - Waiting for previous DNS refresh Task to finish...");
                Thread.Sleep(6000);
            }

            DNSRefreshThread = RefreshDNS();
            DNSRefreshThread.Dispose();
            DNSRefreshThread = null;
        }

        finally
        {
            dnswatcher.EnableRaisingEvents = true;
        }
    }

    private static void StartOrUpdateServer()
    {
        Server.StopServer();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        dnswatcher.Path = Path.GetDirectoryName(MitmDNSServerConfiguration.DNSConfig) ?? configDir;
        dnswatcher.Filter = Path.GetFileName(MitmDNSServerConfiguration.DNSConfig);

        if (File.Exists(MitmDNSServerConfiguration.DNSConfig))
        {
            string MD5 = ComputeMD5FromFile(MitmDNSServerConfiguration.DNSConfig);

            if (!MD5.Equals(DNSconfigMD5))
            {
                DNSconfigMD5 = MD5;

                while (DNSRefreshThread != null)
                {
                    LoggerAccessor.LogWarn("[DNS] - Waiting for previous DNS refresh Task to finish...");
                    Thread.Sleep(6000);
                }

                DNSRefreshThread = RefreshDNS();
                DNSRefreshThread.Dispose();
                DNSRefreshThread = null;
            }
        }

        Server.StartServerAsync(new CancellationTokenSource().Token);
    }

    private static Task RefreshDNS()
    {
        if (DNSThread != null && !MitmDNSClass.Initiated)
        {
            while (!MitmDNSClass.Initiated)
            {
                LoggerAccessor.LogWarn("[DNS] - Waiting for previous config assignement Task to finish...");
                Thread.Sleep(6000);
            }
        }

        DNSThread = Task.Run(MitmDNSClass.RenewConfig);

        return Task.CompletedTask;
    }

    private static string ComputeMD5FromFile(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        // Convert the byte array to a hexadecimal string
        return NetHasher.ComputeMD5String(stream);
    }

    static void Main()
    {
        dnswatcher.NotifyFilter = NotifyFilters.LastWrite;
        dnswatcher.Changed += OnDNSChanged;

        if (!NetworkLibrary.Extension.OtherExtensions.IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        LoggerAccessor.SetupLogger("MitmDNS", Directory.GetCurrentDirectory());

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

        MitmDNSServerConfiguration.RefreshVariables(configPath);

        StartOrUpdateServer();

        dnswatcher.EnableRaisingEvents = true;

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

                            MitmDNSServerConfiguration.RefreshVariables(configPath);

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