using CustomLogger;
using Newtonsoft.Json.Linq;
using SSFWServer;

public static class SSFWServerConfiguration
{
    public static bool SSFWCrossSave { get; set; } = true;
    public static string SSFWMinibase { get; set; } = "[]";
    public static string SSFWLegacyKey { get; set; } = string.Empty;
    public static string SSFWStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwssfwroot";
    public static string SSFWCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/SSFW.pfx";
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
            LoggerAccessor.LogWarn("Could not find the ssfw.json file, using server's default.");
            return;
        }

        // Read the file
        string json = File.ReadAllText(configPath);

        // Parse the JSON configuration
        dynamic config = JObject.Parse(json);

        SSFWMinibase = config.minibase;
        SSFWLegacyKey = config.legacyKey;
        SSFWCrossSave = config.cross_save;
        SSFWStaticFolder = config.static_folder;
        SSFWCertificateFile = config.certificate_file;
        JArray bannedIPsArray = config.BannedIPs;
        // Deserialize BannedIPs if it exists
        if (bannedIPsArray != null)
            BannedIPs = bannedIPsArray.ToObject<List<string>>();
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

            SSFWServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/ssfw.json");
        }
    }

    static void Main()
    {
        LoggerAccessor.SetupLogger("SSFWServer");

        SSFWServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/ssfw.json");

        if (!File.Exists(SSFWServerConfiguration.SSFWCertificateFile))
            CryptoSporidium.SSLUtils.CreateSelfSignedCert(SSFWServerConfiguration.SSFWCertificateFile, "secure.cprod.homeps3.online.scee.com");

        CryptoSporidium.SSLUtils.CreateHomeCertificatesFile(File.ReadAllText(Path.GetDirectoryName(SSFWServerConfiguration.SSFWCertificateFile) + $"/{Path.GetFileNameWithoutExtension(SSFWServerConfiguration.SSFWCertificateFile)}.pem"), Path.GetDirectoryName(SSFWServerConfiguration.SSFWCertificateFile) + "/CERTIFICATES.TXT");

        SSFWClass server = new(SSFWServerConfiguration.SSFWCertificateFile, "qwerty", SSFWServerConfiguration.SSFWLegacyKey);

        _ = Task.Run(server.StartSSFW);

        _ = Task.Run(RefreshConfig);

        if (Misc.IsWindows())
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