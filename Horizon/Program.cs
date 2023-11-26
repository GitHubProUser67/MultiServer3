using CustomLogger;
using CryptoSporidium.Horizon.LIBRARY.Database;
using Horizon.PluginManager;
using Newtonsoft.Json.Linq;
using System.Runtime;

public static class HorizonServerConfiguration
{
    public static string PluginsFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/medius_plugins";
    public static string DatabaseConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/db.config.json";
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
    public static bool EnableMedius { get; set; } = true;
    public static bool EnableDME { get; set; } = true;
    public static bool EnableMuis { get; set; } = true;
    public static bool EnableBWPS { get; set; } = true;
    public static bool EnableNAT { get; set; } = true;
    public static bool UseSonyNAT { get; set; } = true;
    public static string? PlayerAPIStaticPath { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string? DMEConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/dme.json";
    public static string? MEDIUSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/medius.json";
    public static string? MUISConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/muis.json";
    public static string? BWPSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/bwps.json";
    public static string? NATConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/nat.json";
    public static string HomeVersionBetaHDK { get; set; } = "01.86";
    public static string HomeVersionRetail { get; set; } = "01.86";

    public static DbController? Database = new(DatabaseConfig);

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
            LoggerAccessor.LogWarn("Could not find the horizon.json file, using server's default.");
            return;
        }

        try
        {
            // Read the file
            string json = File.ReadAllText(configPath);

            // Parse the JSON configuration
            dynamic config = JObject.Parse(json);

            EnableMedius = config.medius.enabled;
            EnableDME = config.dme.enabled;
            EnableMuis = config.muis.enabled;
            EnableNAT = config.nat.enabled;
            EnableBWPS = config.bwps.enabled;
            HTTPSCertificateFile = config.certificate_file;
            PlayerAPIStaticPath = config.player_api_static_path;
            DMEConfig = config.dme.config;
            MEDIUSConfig = config.medius.config;
            MUISConfig = config.muis.config;
            NATConfig = config.nat.config;
            BWPSConfig = config.bwps.config;
            UseSonyNAT = config.use_sony_nat;
            PluginsFolder = config.plugins_folder;
            DatabaseConfig = config.database;
            HomeVersionBetaHDK = config.home_version_beta_hdk;
            HomeVersionRetail = config.home_version_retail;
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("horizon.json file is malformed, using server's default.");
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

            HorizonServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/horizon.json");
        }
    }

    static Task HorizonStarter()
    {
        if (HorizonServerConfiguration.EnableMedius)
        {
            Horizon.MEDIUS.MediusClass.MediusMain();

            _ = Horizon.HTTPSERVICE.HttpClass.RunServerAsync(61920);

            _ = Horizon.HTTPSERVICE.HttpClass.RunServerAsync(8443, true);
        }

        if (HorizonServerConfiguration.EnableNAT)
            Horizon.NAT.NATClass.NATMain();

        if (HorizonServerConfiguration.EnableBWPS)
            Horizon.BWPS.BWPSClass.BWPSMain();

        if (HorizonServerConfiguration.EnableMuis)
            Horizon.MUIS.MuisClass.MuisMain();

        if (HorizonServerConfiguration.EnableDME)
            Horizon.DME.DmeClass.DmeMain();

        return Task.CompletedTask;
    }

    static void Main()
    {
        if (!CryptoSporidium.MiscUtils.IsWindows())
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("Horizon");

        HorizonServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/horizon.json");

        CryptoSporidium.SSLUtils.InitCerts(HorizonServerConfiguration.HTTPSCertificateFile);

        _ = Task.Run(HorizonStarter);

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