using CustomLogger;
using Newtonsoft.Json.Linq;
using HTTPSecureServerLite;

public static class HTTPSServerConfiguration
{
    public static string HTTPSStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwroot";
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
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
            LoggerAccessor.LogWarn("Could not find the https.json file, using server's default.");
            return;
        }

        // Read the file
        string json = File.ReadAllText(configPath);

        // Parse the JSON configuration
        dynamic config = JObject.Parse(json);

        HTTPSStaticFolder = config.https_static_folder;
        HTTPSCertificateFile = config.certificate_file;
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

            HTTPSServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/https.json");
        }
    }

    static void Main()
    {
        LoggerAccessor.SetupLogger("HTTPSecureServer");

        HTTPSServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/https.json");

        CryptoSporidium.SSLUtils.InitCerts(HTTPSServerConfiguration.HTTPSCertificateFile);

        Processor server = new(HTTPSServerConfiguration.HTTPSCertificateFile, "qwerty", "*", 443);

        server.StartServer();

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