using SVO;
using CustomLogger;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Horizon.LIBRARY.Database;

public static class SVOServerConfiguration
{
    public static string DatabaseConfig { get; set; } = $"{Directory.GetCurrentDirectory()}/static/db.config.json";
    public static string? SVOStaticFolder { get; set; } = $"{Directory.GetCurrentDirectory()}/static/wwwsvoroot";
    public static bool SVOHTTPSBypass { get; set; } = true;
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

    public static DbController? Database = null;

    public static void SetupDatabase()
    {
        Database = new(DatabaseConfig);
    }

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
            LoggerAccessor.LogWarn("Could not find the svo.json file, using server's default.");
            return;
        }

        // Read the file
        string json = File.ReadAllText(configPath);

        // Parse the JSON configuration
        dynamic config = JObject.Parse(json);

        SVOStaticFolder = config.static_folder;
        SVOHTTPSBypass = config.https_bypass;
        DatabaseConfig = config.database;

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
        if (Misc.IsWindows())
            if (!Misc.IsAdministrator())
            {
                Console.WriteLine("Trying to restart as admin");
                if (Misc.StartAsAdmin(Process.GetCurrentProcess().MainModule.FileName))
                    Environment.Exit(0);
            }

        LoggerAccessor.SetupLogger("SVO");

        SVOServerConfiguration.RefreshVariables($"{Directory.GetCurrentDirectory()}/static/svo.json");
		
		SVOServerConfiguration.SetupDatabase();

        Processor server = new("*", 10060);

        server.Start();

        _ = Task.Run(SVOManager.StartTickPooling);

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