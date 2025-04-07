using CustomLogger;
using Horizon.LIBRARY.Database;
using Horizon.PluginManager;
using Newtonsoft.Json.Linq;
using CyberBackendLibrary.GeoLocalization;
using System.Runtime;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using Horizon.HTTPSERVICE;
using Horizon.MUM;

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
    public static string? DMEConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/dme.json";
    public static string? MEDIUSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/medius.json";
    public static string? MUISConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/muis.json";
    public static string? BWPSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/bwps.json";
    public static string? NATConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/nat.json";
    public static string MediusAPIKey { get; set; } = "nwnbiRsiohjuUHQfPaNrStG3moQZH+deR8zIykB8Lbc="; // Base64 only.
    public static string HomeVersionBetaHDK { get; set; } = "01.86";
    public static string HomeVersionRetail { get; set; } = "01.86";
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
                new JProperty("https_dns_list", HTTPSDNSList ?? Array.Empty<string>()),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("certificate_password", HTTPSCertificatePassword),
                new JProperty("certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name),
                new JProperty("player_api_static_path", PlayerAPIStaticPath),
                new JProperty("medius_api_key", MediusAPIKey),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("database", DatabaseConfig),
                new JProperty("home_version_beta_hdk", HomeVersionBetaHDK),
                new JProperty("home_version_retail", HomeVersionRetail)
            ).ToString().Replace("/", "\\\\"));

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
            MediusAPIKey = GetValueOrDefault(config, "medius_api_key", MediusAPIKey);
            PluginsFolder = GetValueOrDefault(config, "plugins_folder", PluginsFolder);
            DatabaseConfig = GetValueOrDefault(config, "database", DatabaseConfig);
            HomeVersionBetaHDK = GetValueOrDefault(config, "home_version_beta_hdk", HomeVersionBetaHDK);
            HomeVersionRetail = GetValueOrDefault(config, "home_version_retail", HomeVersionRetail);
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"horizon.json file is malformed (exception: {ex}), using server's default.");
        }
    }

    // Helper method to get a value or default value if not present
    public static T GetValueOrDefault<T>(dynamic obj, string propertyName, T defaultValue)
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
            else if (obj is JArray jArray)
            {
                if (int.TryParse(propertyName, out int index) && index >= 0 && index < jArray.Count)
                {
                    T? returnvalue = jArray[index].ToObject<T>();
                    if (returnvalue != null)
                        return returnvalue;
                }
            }
        }
        return defaultValue;
    }
}

class Program
{
    static string configDir = Directory.GetCurrentDirectory() + "/static/";
    static string configPath = configDir + "horizon.json";
    static bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
    static ConcurrentBag<CrudServerHandler>? HTTPBag;
    static MumServerHandler? MUMServer;

    static Task HorizonStarter()
    {
        if (HorizonServerConfiguration.EnableMedius)
        {
            MUMServer = new MumServerHandler("*", 10076);

            HTTPBag = new ConcurrentBag<CrudServerHandler>
            {
                new("*", 61920),
                new("0.0.0.0", 8443, HorizonServerConfiguration.HTTPSCertificateFile, HorizonServerConfiguration.HTTPSCertificatePassword)
            };
        }

        if (HorizonServerConfiguration.EnableMedius && !Horizon.MEDIUS.MediusClass.started)
            Horizon.MEDIUS.MediusClass.StartServer();

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

    static void StartOrUpdateServer()
    {
        if (Horizon.DME.DmeClass.started && !HorizonServerConfiguration.EnableDME)
            Horizon.DME.DmeClass.StopServer();

        if (Horizon.MUIS.MuisClass.started && !HorizonServerConfiguration.EnableMuis)
            Horizon.MUIS.MuisClass.StopServer();

        if (Horizon.BWPS.BWPSClass.started && !HorizonServerConfiguration.EnableBWPS)
            Horizon.BWPS.BWPSClass.StopServer();

        if (Horizon.NAT.NATClass.started && !HorizonServerConfiguration.EnableNAT)
            Horizon.NAT.NATClass.StopServer();

        if (Horizon.MEDIUS.MediusClass.started && !HorizonServerConfiguration.EnableMedius)
            Horizon.MEDIUS.MediusClass.StopServer();

        MUMServer?.StopServer();
        MUMServer = null;

        if (HTTPBag != null)
        {
            foreach (var httpBag in HTTPBag)
            {
                httpBag.StopServer();
            }

            HTTPBag = null;
        }

        if (HorizonServerConfiguration.EnableMedius)
            CyberBackendLibrary.SSL.SSLUtils.InitCerts(HorizonServerConfiguration.HTTPSCertificateFile, HorizonServerConfiguration.HTTPSCertificatePassword,
                    HorizonServerConfiguration.HTTPSDNSList, HorizonServerConfiguration.HTTPSCertificateHashingAlgorithm);

        HorizonStarter().Wait();
    }

    static string ComputeMD5FromFile(string filePath)
    {
        using (FileStream stream = File.OpenRead(filePath))
        {
            // Convert the byte array to a hexadecimal string
            return BitConverter.ToString(MD5.Create().ComputeHash(stream)).Replace("-", string.Empty);
        }
    }

    static void Main()
    {
        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("Horizon");

        GeoIP.Initialize();

        HorizonServerConfiguration.RefreshVariables(configPath);

        StartOrUpdateServer();

        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true")
        {
            LoggerAccessor.LogInfo("Console Inputs are now available while server is running. . .");

            while (true)
            {
                string? stdin = Console.ReadLine();

                if (!string.IsNullOrEmpty(stdin))
                {
                    switch (stdin.ToLower())
                    {
                        case "shutdown":
                            LoggerAccessor.LogWarn("Are you sure you want to shut down the server? [y/N]");

                            if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                            {
                                LoggerAccessor.LogInfo("Shutting down. Goodbye!");
                                Environment.Exit(0);
                            }
                            break;
                        case "reboot":
                            LoggerAccessor.LogWarn("Are you sure you want to reboot the server? [y/N]");

                            if (char.ToLower(Console.ReadKey().KeyChar) == 'y')
                            {
                                LoggerAccessor.LogInfo("Rebooting!");

                                HorizonServerConfiguration.RefreshVariables(configPath);

                                StartOrUpdateServer();
                            }
                            break;
                        default:
                            LoggerAccessor.LogWarn($"Unknown command entered: {stdin}");
                            break;
                    }
                }
                else
                    LoggerAccessor.LogWarn("No command entered!");
            }
        }
        else
        {
            LoggerAccessor.LogWarn("\nConsole Inputs are locked while server is running. . .");

            Thread.Sleep(Timeout.Infinite);
        }
    }
}