using CustomLogger;
using Horizon.LIBRARY.Database;
using Horizon.PluginManager;
using Newtonsoft.Json.Linq;
using NetworkLibrary.GeoLocalization;
using System.Runtime;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using Horizon.HTTPSERVICE;
using Horizon.MUM;
using System.Reflection;
using NetworkLibrary.TCP_IP;
using NetworkLibrary.Extension;

public static class HorizonServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/medius_plugins";
    public static string DatabaseConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/db.config.json";
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/HorizonHTTPService.pfx";
    public static string HTTPSCertificatePassword { get; set; } = "qwerty";
    public static HashAlgorithmName HTTPSCertificateHashingAlgorithm { get; set; } = HashAlgorithmName.SHA384;
    public static bool EnableMedius { get; set; } = true;
    public static bool EnableDME { get; set; } = true;
    public static bool EnableMuis { get; set; } = true;
    public static bool EnableBWPS { get; set; } = true;
    public static bool EnableNAT { get; set; } = true;
    public static string? PlayerAPIStaticPath { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string? EBOOTDEFSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/ebootdefs.json";
    public static string? DMEConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/dme.json";
    public static string? MEDIUSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/medius.json";
    public static string? MUISConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/muis.json";
    public static string? BWPSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/bwps.json";
    public static string? NATConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/nat.json";
    public static string MediusAPIKey { get; set; } = "nwnbiRsiohjuUHQfPaNrStG3moQZH+deR8zIykB8Lbc=";
    public static string[]? HTTPSDNSList { get; set; }

    public static DbController Database = new(DatabaseConfig);

    public static List<IPlugin> plugins = PluginLoader.LoadPluginsFromFolder(PluginsFolder);

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
            LoggerAccessor.LogWarn("Could not find the horizon.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("medius", new JObject(
                    new JProperty("enabled", EnableMedius),
                    new JProperty("config", MEDIUSConfig)
                )),
                new JProperty("dme", new JObject(
                    new JProperty("enabled", EnableDME),
                    new JProperty("config", DMEConfig)
                )),
                new JProperty("muis", new JObject(
                    new JProperty("enabled", EnableMuis),
                    new JProperty("config", MUISConfig)
                )),
                new JProperty("nat", new JObject(
                    new JProperty("enabled", EnableNAT),
                    new JProperty("config", NATConfig)
                )),
                new JProperty("bwps", new JObject(
                    new JProperty("enabled", EnableBWPS),
                    new JProperty("config", BWPSConfig)
                )),
                new JProperty("eboot_defs_config", EBOOTDEFSConfig),
                new JProperty("https_dns_list", HTTPSDNSList ?? Array.Empty<string>()),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("certificate_password", HTTPSCertificatePassword),
                new JProperty("certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name),
                new JProperty("player_api_static_path", PlayerAPIStaticPath),
                new JProperty("medius_api_key", MediusAPIKey),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("database", DatabaseConfig)
            ).ToString());

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            EnableMedius = GetValueOrDefault(config.medius, "enabled", EnableMedius);
            EnableDME = GetValueOrDefault(config.dme, "enabled", EnableDME);
            EnableMuis = GetValueOrDefault(config.muis, "enabled", EnableMuis);
            EnableNAT = GetValueOrDefault(config.nat, "enabled", EnableNAT);
            EnableBWPS = GetValueOrDefault(config.bwps, "enabled", EnableBWPS);
            HTTPSCertificateFile = GetValueOrDefault(config, "certificate_file", HTTPSCertificateFile);
            HTTPSCertificatePassword = GetValueOrDefault(config, "certificate_password", HTTPSCertificatePassword);
            HTTPSCertificateHashingAlgorithm = new HashAlgorithmName(GetValueOrDefault(config, "certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name));
            PlayerAPIStaticPath = GetValueOrDefault(config, "player_api_static_path", PlayerAPIStaticPath);
            HTTPSDNSList = GetValueOrDefault(config, "https_dns_list", HTTPSDNSList);
            DMEConfig = GetValueOrDefault(config.dme, "config", DMEConfig);
            MEDIUSConfig = GetValueOrDefault(config.medius, "config", MEDIUSConfig);
            MUISConfig = GetValueOrDefault(config.muis, "config", MUISConfig);
            NATConfig = GetValueOrDefault(config.nat, "config", NATConfig);
            BWPSConfig = GetValueOrDefault(config.bwps, "config", BWPSConfig);
            EBOOTDEFSConfig = GetValueOrDefault(config.bwps, "eboot_defs_config", EBOOTDEFSConfig);
            string APIKey = GetValueOrDefault(config, "medius_api_key", MediusAPIKey);
            if (OtherExtensions.IsBase64String(APIKey))
                MediusAPIKey = APIKey;
            PluginsFolder = GetValueOrDefault(config, "plugins_folder", PluginsFolder);
            DatabaseConfig = GetValueOrDefault(config, "database", DatabaseConfig);
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"horizon.json file is malformed (exception: {ex}), using server's default.");
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
    private static string configPath = configDir + "horizon.json";
    private static ConcurrentBag<CrudServerHandler>? HTTPBag;
    private static MumServerHandler? MUMServer;

    private static Task HorizonStarter()
    {
        if (HorizonServerConfiguration.EnableMedius)
        {
            try
            {
                MUMServer = new MumServerHandler("*", 10076);

                HTTPBag = new ConcurrentBag<CrudServerHandler>
                {
                    new("*", 61920),
                    new("*", 8443, HorizonServerConfiguration.HTTPSCertificateFile, HorizonServerConfiguration.HTTPSCertificatePassword)
                };
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError("[HTTPSERVICE] - An exception was thrown while starting the Medius HTTP Services: " + ex);
            }
        }

        if (HorizonServerConfiguration.EnableMedius && !Horizon.SERVER.MediusClass.started)
            Horizon.SERVER.MediusClass.StartServer();

        if (HorizonServerConfiguration.EnableNAT && !Horizon.NAT.NATClass.started)
            Horizon.NAT.NATClass.StartServer();

        if (HorizonServerConfiguration.EnableBWPS && !Horizon.BWPS.BWPSClass.started)
            Horizon.BWPS.BWPSClass.StartServer();

        if (HorizonServerConfiguration.EnableMuis && !Horizon.MUIS.MuisClass.started)
            Horizon.MUIS.MuisClass.StartServer();

        if (HorizonServerConfiguration.EnableDME && !Horizon.DME.DmeClass.started)
            Horizon.DME.DmeClass.StartServer();

        return Task.CompletedTask;
    }

    private static void StartOrUpdateServer()
    {
        if (Horizon.DME.DmeClass.started && !HorizonServerConfiguration.EnableDME)
            Horizon.DME.DmeClass.StopServer();

        if (Horizon.MUIS.MuisClass.started && !HorizonServerConfiguration.EnableMuis)
            Horizon.MUIS.MuisClass.StopServer();

        if (Horizon.BWPS.BWPSClass.started && !HorizonServerConfiguration.EnableBWPS)
            Horizon.BWPS.BWPSClass.StopServer();

        if (Horizon.NAT.NATClass.started && !HorizonServerConfiguration.EnableNAT)
            Horizon.NAT.NATClass.StopServer();

        if (Horizon.SERVER.MediusClass.started && !HorizonServerConfiguration.EnableMedius)
            Horizon.SERVER.MediusClass.StopServer();

        MUMServer?.StopServer();

        if (HTTPBag != null)
        {
            foreach (var httpBag in HTTPBag)
            {
                httpBag.StopServer();
            }
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        if (HorizonServerConfiguration.EnableMedius)
            NetworkLibrary.SSL.SSLUtils.InitializeSSLCertificates(HorizonServerConfiguration.HTTPSCertificateFile, HorizonServerConfiguration.HTTPSCertificatePassword,
                    HorizonServerConfiguration.HTTPSDNSList, HorizonServerConfiguration.HTTPSCertificateHashingAlgorithm);

        HorizonStarter().Wait();
    }

    static void Main()
    {
        if (!OtherExtensions.IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        LoggerAccessor.SetupLogger("Horizon", Directory.GetCurrentDirectory());

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

        GeoIP.Initialize();

        HorizonServerConfiguration.RefreshVariables(configPath);

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

                            HorizonServerConfiguration.RefreshVariables(configPath);

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