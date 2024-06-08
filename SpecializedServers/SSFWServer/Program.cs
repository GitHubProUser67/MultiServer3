using CustomLogger;
using Newtonsoft.Json.Linq;
using SSFWServer;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;

public static class SSFWServerConfiguration
{
    public static bool SSFWCrossSave { get; set; } = true;
    public static string SSFWMinibase { get; set; } = "[]";
    public static string SSFWLegacyKey { get; set; } = "**NoNoNoYouCantHaxThis****69";
    public static string SSFWStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwssfwroot";
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/SSFW.pfx";
    public static string HTTPSCertificatePassword { get; set; } = "qwerty";
    public static HashAlgorithmName HTTPSCertificateHashingAlgorithm { get; set; } = HashAlgorithmName.SHA384;
    public static string ScenelistFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwssfwroot/SceneList.xml";
    public static string[]? HTTPSDNSList { get; set; } = {
            "cprod.homerewards.online.scee.com",
            "cprod.homeidentity.online.scee.com",
            "cprod.homeserverservices.online.scee.com",
        };
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
            LoggerAccessor.LogWarn("Could not find the ssfw.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("minibase", SSFWMinibase),
                new JProperty("legacyKey", SSFWLegacyKey),
                new JProperty("cross_save", SSFWCrossSave),
                new JProperty("static_folder", SSFWStaticFolder),
                new JProperty("https_dns_list", HTTPSDNSList ?? Array.Empty<string>()),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("certificate_password", HTTPSCertificatePassword),
                new JProperty("certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name),
                new JProperty("scenelist_file", ScenelistFile),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { }))
            ).ToString());

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            SSFWMinibase = GetValueOrDefault(config, "minibase", SSFWMinibase);
            SSFWLegacyKey = GetValueOrDefault(config, "legacyKey", SSFWLegacyKey);
            SSFWCrossSave = GetValueOrDefault(config, "cross_save", SSFWCrossSave);
            SSFWStaticFolder = GetValueOrDefault(config, "static_folder", SSFWStaticFolder);
            HTTPSCertificateFile = GetValueOrDefault(config, "certificate_file", HTTPSCertificateFile);
            HTTPSCertificatePassword = GetValueOrDefault(config, "certificate_password", HTTPSCertificatePassword);
            HTTPSCertificateHashingAlgorithm = new HashAlgorithmName(GetValueOrDefault(config, "certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name));
            HTTPSDNSList = GetValueOrDefault(config, "https_dns_list", HTTPSDNSList);
            ScenelistFile = GetValueOrDefault(config, "scenelist_file", ScenelistFile);
            // Deserialize BannedIPs if it exists
            try
            {
                JArray bannedIPsArray = config.BannedIPs;
                // Deserialize BannedIPs if it exists
                if (bannedIPsArray != null)
                    BannedIPs = bannedIPsArray.ToObject<List<string>>();
            }
            catch
            {

            }
        }
        catch (Exception ex)
        {
            LoggerAccessor.LogWarn($"ssfw.json file is malformed (exception: {ex}), using server's default.");
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
    private static string configDir = Directory.GetCurrentDirectory() + "/static/";
    private static string configPath = configDir + "ssfw.json";
    private static bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;
    private static SSFWClass? Server;

    private static void StartOrUpdateServer()
    {
        Server?.StopSSFW();
        Server = null;

        CyberBackendLibrary.SSL.SSLUtils.InitCerts(SSFWServerConfiguration.HTTPSCertificateFile, SSFWServerConfiguration.HTTPSCertificatePassword,
            SSFWServerConfiguration.HTTPSDNSList, SSFWServerConfiguration.HTTPSCertificateHashingAlgorithm);

        Server = new SSFWClass(SSFWServerConfiguration.HTTPSCertificateFile, SSFWServerConfiguration.HTTPSCertificatePassword, SSFWServerConfiguration.SSFWLegacyKey);

        Server.StartSSFW();
    }

    static void Main()
    {
        if (!IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        LoggerAccessor.SetupLogger("SSFWServer", Directory.GetCurrentDirectory());

        SSFWServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/ssfw.json");

        _ = new Timer(ScenelistParser.UpdateSceneDictionary, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

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

                            SSFWServerConfiguration.RefreshVariables(configPath);

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