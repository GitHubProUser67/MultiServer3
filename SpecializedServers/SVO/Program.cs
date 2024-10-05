using SVO;
using CustomLogger;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Horizon.LIBRARY.Database;
using System.Runtime;
using System.Net;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Reflection;
using NetworkLibrary.TCP_IP;

public static class SVOServerConfiguration
{
    public static string DatabaseConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/db.config.json";
    public static string HTTPSCertificateFile { get; set; } = $"{Directory.GetCurrentDirectory()}/static/SSL/SVO.pfx";
    public static string HTTPSCertificatePassword { get; set; } = "qwerty";
    public static HashAlgorithmName HTTPSCertificateHashingAlgorithm { get; set; } = HashAlgorithmName.SHA384;
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

    public static string[]? HTTPSDNSList { get; set; } = {
            "homeps3.svo.online.scee.com",
            "starhawk-prod2.svo.online.scea.com",
            "warhawk-prod3.svo.online.scea.com",
            "singstar.svo.online.com",
            "hdc.cprod.homeps3.online.scee.com",
            "secure.cprodts.homeps3.online.scee.com",
			"wipeout2048.online.scee.com"
        };

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
                new JProperty("https_dns_list", HTTPSDNSList ?? Array.Empty<string>()),
                new JProperty("certificate_file", HTTPSCertificateFile),
                new JProperty("certificate_password", HTTPSCertificatePassword),
                new JProperty("certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name),
                new JProperty("database", DatabaseConfig),
                new JProperty("pshome_rpcs3workaround", PSHomeRPCS3Workaround),
                new JProperty("MOTD", string.Empty),
                new JProperty("BannedIPs", new JArray(BannedIPs ?? new List<string> { }))
            ).ToString());

            return;
        }

        try
        {
            // Parse the JSON configuration
            dynamic config = JObject.Parse(File.ReadAllText(configPath));

            SVOStaticFolder = GetValueOrDefault(config, "static_folder", SVOStaticFolder);
            SVOHTTPSBypass = GetValueOrDefault(config, "https_bypass", SVOHTTPSBypass);
            HTTPSCertificateFile = GetValueOrDefault(config, "certificate_file", HTTPSCertificateFile);
            HTTPSCertificatePassword = GetValueOrDefault(config, "certificate_password", HTTPSCertificatePassword);
            HTTPSCertificateHashingAlgorithm = new HashAlgorithmName(GetValueOrDefault(config, "certificate_hashing_algorithm", HTTPSCertificateHashingAlgorithm.Name));
            HTTPSDNSList = GetValueOrDefault(config, "https_dns_list", HTTPSDNSList);
            DatabaseConfig = GetValueOrDefault(config, "database", DatabaseConfig);
            HTTPSCertificateFile = GetValueOrDefault(config, "certificate_file", HTTPSCertificateFile);
            PSHomeRPCS3Workaround = GetValueOrDefault(config, "pshome_rpcs3workaround", PSHomeRPCS3Workaround);
            // Look for the MOTD xml file.
            string motd_file = GetValueOrDefault(config, "MOTD", string.Empty);
            if (string.IsNullOrEmpty(motd_file) || !File.Exists(motd_file))
                LoggerAccessor.LogWarn("Could not find the MOTD file, using default xml.");
            else
                MOTD = File.ReadAllText(motd_file);
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
            LoggerAccessor.LogWarn($"svo.json file is malformed (exception: {ex}), using server's default.");
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
    private static string configPath = configDir + "svo.json";
    private static Task? MediusDatabaseLoop;
    private static OTGSecureServerLite? OTGServer;
    private static SVOServer? _SVOServer;

    private static void StartOrUpdateServer()
    {
        OTGServer?.StopServer();
        _SVOServer?.Stop();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        NetworkLibrary.SSL.SSLUtils.InitializeSSLCertificates(SVOServerConfiguration.HTTPSCertificateFile, SVOServerConfiguration.HTTPSCertificatePassword,
            SVOServerConfiguration.HTTPSDNSList, SVOServerConfiguration.HTTPSCertificateHashingAlgorithm);

        MediusDatabaseLoop ??= Task.Run(SVOManager.StartTickPooling);

        if (HttpListener.IsSupported)
            _SVOServer = new SVOServer("*", new System.Security.Cryptography.X509Certificates.X509Certificate2(SVOServerConfiguration.HTTPSCertificateFile, SVOServerConfiguration.HTTPSCertificatePassword));
        else
            LoggerAccessor.LogError("Windows XP SP2 or Server 2003 is required to use the HttpListener class, so SVO Server not started!");

        OTGServer = new OTGSecureServerLite(SVOServerConfiguration.HTTPSCertificateFile, SVOServerConfiguration.HTTPSCertificatePassword, "*", 10062);
    }

    static void Main()
    {
        if (!NetworkLibrary.Extension.OtherExtensions.IsWindows)
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        else
            TechnitiumLibrary.Net.Firewall.FirewallHelper.CheckFirewallEntries(Assembly.GetEntryAssembly()?.Location);

        LoggerAccessor.SetupLogger("SVO", Directory.GetCurrentDirectory());

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

        SVOServerConfiguration.RefreshVariables(configPath);

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

                            SVOServerConfiguration.RefreshVariables(configPath);

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
            
        }

        return false;
    }
}