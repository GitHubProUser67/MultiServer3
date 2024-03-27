using CustomLogger;
using Horizon.LIBRARY.Database;
using Horizon.PluginManager;
using Newtonsoft.Json.Linq;
using BackendProject.MiscUtils;

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
    public static bool EnableDiscordPlugin { get; set; } = true;
    public static string? PlayerAPIStaticPath { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string? DMEConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/dme.json";
    public static string? MEDIUSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/medius.json";
    public static string? MUISConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/muis.json";
    public static string? BWPSConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/bwps.json";
    public static string? NATConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/nat.json";
    public static string MediusAPIKey { get; set; } = "nwnbiRsiohjuUHQfPaNrStG3moQZH+deR8zIykB8Lbc="; // Base64 only.
    public static string HomeVersionBetaHDK { get; set; } = "01.86";
    public static string HomeVersionRetail { get; set; } = "01.86";
    public static string DiscordBotToken { get; set; } = string.Empty;
    public static string DiscordChannelID { get; set; } = string.Empty;

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

            Directory.CreateDirectory(Path.GetDirectoryName(configPath));

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
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("player_api_static_path", PlayerAPIStaticPath),
                new JProperty("medius_api_key", MediusAPIKey),
                new JProperty("plugins_folder", PluginsFolder),
                new JProperty("database", DatabaseConfig),
                new JProperty("home_version_beta_hdk", HomeVersionBetaHDK),
                new JProperty("home_version_retail", HomeVersionRetail),
                new JProperty("discord_bot_token", DiscordBotToken),
                new JProperty("discord_channel_id", DiscordChannelID),
                new JProperty("discord_plugin", new JObject(
                    new JProperty("enabled", EnableDiscordPlugin)
                ))
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

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
            MediusAPIKey = config.medius_api_key;
            PluginsFolder = config.plugins_folder;
            DatabaseConfig = config.database;
            HomeVersionBetaHDK = config.home_version_beta_hdk;
            HomeVersionRetail = config.home_version_retail;
            DiscordBotToken = config.discord_bot_token;
            DiscordChannelID = config.discord_channel_id;
            EnableDiscordPlugin = config.discord_plugin.enabled;
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
            GeoIPUtils.Initialize();

            Task.Run(() => { new Horizon.MUM.MumServerHandler("*", 10076).StartServer(); });

            Horizon.MEDIUS.MediusClass.MediusMain();

            _ = Task.Run(() => Parallel.Invoke(
                   () => new Horizon.HTTPSERVICE.CrudServerHandler("*", 61920).StartServer(),
                   () => new Horizon.HTTPSERVICE.CrudServerHandler("*", 8443).StartServer(HorizonServerConfiguration.HTTPSCertificateFile)
               ));
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
        LoggerAccessor.SetupLogger("Horizon");

        HorizonServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/horizon.json");

        SSLUtils.InitCerts(HorizonServerConfiguration.HTTPSCertificateFile);

        if (HorizonServerConfiguration.EnableDiscordPlugin && !string.IsNullOrEmpty(HorizonServerConfiguration.DiscordChannelID) && !string.IsNullOrEmpty(HorizonServerConfiguration.DiscordBotToken))
            _ = BackendProject.Discord.CrudDiscordBot.BotStarter(HorizonServerConfiguration.DiscordChannelID, HorizonServerConfiguration.DiscordBotToken);

        _ = Task.Run(() => Parallel.Invoke(
                    () => HorizonStarter(),
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