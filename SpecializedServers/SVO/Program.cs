using SVO;
using CustomLogger;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Horizon.LIBRARY.Database;
using System.Runtime;
using System.Net;

using System.Security.Principal;

public static class SVOServerConfiguration
{
    public static string DatabaseConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/db.config.json";
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/MultiServer.pfx";
    public static string SVOStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwsvoroot";
    public static bool SVOHTTPSBypass { get; set; } = true;
    public static bool PSHomeRPCS3Workaround { get; set; } = true;
    public static string? MOTD { get; set; } = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<SVML>\r\n" +
        "    <RECTANGLE class=\"CHIP_FACE\" name=\"backPanel\" x=\"292\" y=\"140\" width=\"708\" height=\"440\"/>\r\n" +
        "    <RECTANGLE class=\"CHIP_RECESS\" name=\"backPanel\" x=\"300\" y=\"148\" width=\"692\" height=\"384\" fillColor=\"#FFFFFFFF\"/>\r\n\r\n" +
        "    <TEXT name=\"text\" x=\"640\" y=\"171\" width=\"636\" height=\"26\" fontSize=\"26\" align=\"center\" textColor=\"#cc000000\">Message Of the Day</TEXT>\r\n\r\n" +
        "    <TEXTAREA class=\"TEXTAREA1\" name=\"message\" x=\"308\" y=\"204\" width=\"664\" height=\"320\"\r\n\t\tfontSize=\"22\" lineSpacing=\"22\" linesVisible=\"14\"\r\n\t\t" +
        "readonly=\"true\" selectable=\"false\" blinkCursor=\"false\"\r\n\t\t" +
        "textColor=\"#CC000000\" highlightTextColor=\"#FF000000\"\r\n\t\t" +
        "leftPadValue=\"8\" topPadValue=\"8\" \r\n" +
        "        defaultTextEntry=\"1\" defaultTextScroll=\"1\">Welcome to PlayStationÂ®Home Open Beta.\r\n\r\n" +
        "Head over to the new Resident Evil 5 Studio Lot space, accessible via the Menu Pad by selecting Locations &gt; World Map and then clicking on the Capcom chip. Here you can enjoy an interactive behind-the-scenes look at the tools and devices used on location for the filming of a portion of Resident Evil 5.\r\n\r\n" +
        "CydoniaX (PlayStationÂ®Home Community Manager) &amp; Locust_Star (PlayStationÂ®Home Community Specialist)</TEXTAREA>\r\n    \r\n    <TEXT name=\"legend\" x=\"984\" y=\"548\" width=\"652\" height=\"18\" fontSize=\"18\" align=\"right\" textColor=\"#CCFFFFFF\">[CROSS] Continue</TEXT>\r\n" +
        "    <QUICKLINK name=\"refresh\" button=\"SV_PAD_X\" linkOption=\"NORMAL\" href=\"../home/homeEnterWorld.jsp\"/>\r\n" +
        "</SVML>";

    public static DbController? Database = new(DatabaseConfig);

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
            LoggerAccessor.LogWarn("Could not find the svo.json file, writing and using server's default.");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory() + "/static");

            // Write the JObject to a file
            File.WriteAllText(configPath, new JObject(
                new JProperty("static_folder", SVOStaticFolder),
                new JProperty("https_bypass", SVOHTTPSBypass),
                new JProperty("database", DatabaseConfig),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("pshome_rpcs3workaround", PSHomeRPCS3Workaround),
                new JProperty("MOTD", MOTD),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { }))
            ).ToString().Replace("/", "\\\\"));

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            SVOStaticFolder = config.static_folder;
            SVOHTTPSBypass = config.https_bypass;
            DatabaseConfig = config.database;
            HTTPSCertificateFile = config.certificate_file;
            PSHomeRPCS3Workaround = config.pshome_rpcs3workaround;
            // Look for the MOTD xml file.
            string motd_file = config.motd_file;
            if (!File.Exists(motd_file))
                LoggerAccessor.LogWarn("Could not find the motd.xml file, using default xml.");
            else
                MOTD = File.ReadAllText(motd_file);
            JArray bannedIPsArray = config.BannedIPs;
            // Deserialize BannedIPs if it exists
            if (bannedIPsArray != null)
                BannedIPs = bannedIPsArray.ToObject<List<string>>();
        }
        catch (Exception)
        {
            LoggerAccessor.LogWarn("svo.json file is malformed, using server's default.");
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

            SVOServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/svo.json");
        }
    }

    static void Main()
    {
        bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32S || Environment.OSVersion.Platform == PlatformID.Win32Windows;

        if (IsWindows)
            if (!IsAdministrator())
            {
                Console.WriteLine("Trying to restart as admin");
                if (StartAsAdmin(Process.GetCurrentProcess().MainModule?.FileName))
                    Environment.Exit(0);
            }
        else
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

        LoggerAccessor.SetupLogger("SVO");

        SVOServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/svo.json");

        CyberBackendLibrary.SSL.SSLUtils.InitCerts(SVOServerConfiguration.HTTPSCertificateFile);

        if (HttpListener.IsSupported)
            _ = Task.Run(new SVOServer("*").Start);
        else
            LoggerAccessor.LogWarn("Windows XP SP2 or Server 2003 is required to use the HttpListener class, so SVO HTTP Server not started.");

        _ = Task.Run(() => Parallel.Invoke(
                    () => new OTGSecureServerLite(Path.GetDirectoryName(SVOServerConfiguration.HTTPSCertificateFile) + $"/{Path.GetFileNameWithoutExtension(SVOServerConfiguration.HTTPSCertificateFile)}_selfsigned.pfx", "qwerty", "0.0.0.0", 10062).StartServer(), // 0.0.0.0 as the certificate binds to this ip.
                    async () => await SVOManager.StartTickPooling(),
                    () => RefreshConfig()
                ));

        if (IsWindows)
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

    /// <summary>
    /// Know if we are the true administrator of the Windows system.
    /// <para>Savoir si est réellement l'administrateur Windows.</para>
    /// </summary>
    /// <returns>A boolean.</returns>
#pragma warning disable
    private static bool IsAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }
#pragma warning restore

    private static bool StartAsAdmin(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        try
        {
            new Process()
            {
                StartInfo =
                    {
                        FileName = filePath,
                        UseShellExecute = true,
                        Verb = "runas"
                    }
            }.Start();

            return true;
        }
        catch
        {
            return false;
        }
    }
}