using PSMultiServer.Addons.Horizon.Server.Common.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace PSMultiServer
{
    public static class ServerConfiguration
    {
		public static string CLIENT_ID { get; set; } = null;
        public static string CLIENT_SECRET { get; set; } = null;
        public static string REFRESH_TOKEN { get; set; } = null;
        public static string PHPVersion { get; set; } = "8.25";
        public static string PHPStaticFolder { get; set; } = "/static/PHP/";
        public static bool PHPDebugErrors { get; set; } = false;
		public static string DNSConfig { get; set; } = "static/routes.txt";
        public static string DNSOnlineConfig { get; set; } = "";
        public static int DNSPort { get; set; } = 53;
        public static string? SSFWPrivateKey { get; set; }
        public static bool SSFWCrossSave { get; set; } = true;
        public static string? SSFWStaticFolder { get; set; } = "/static/wwwssfwroot/";
        public static string? HTTPPrivateKey { get; set; }
		public static bool HTTPS { get; set; } = false;
        public static string? HTTPSBindIp { get; set; }
        public static DateTimeOffset SslRootValidAfter { get; set; } = DateTimeOffset.Now;
        public static DateTimeOffset SslRootValidBefore { get; set; } = DateTimeOffset.Now.AddYears(30);
        public static int SslCertValidAfterNow { get; set; } = 7;
        public static int SslCertValidBeforeNow { get; set; } = -7;
        public static int HTTPPort { get; set; } = 80;
        public static string? HTTPStaticFolder { get; set; } = "/static/wwwroot/";
        public static string SSFWMinibase { get; set; } = "[]";
        public static string VersionBetaHDK { get; set; } = "01.60";
        public static string VersionRetail { get; set; } = "01.83";
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
		public static bool EnableDNSServer { get; set; } = true;
        public static bool EnableHttpServer { get; set; } = true;
        public static bool EnableSSFW { get; set; } = true;
        public static bool EnableMedius { get; set; } = true;
        public static bool MediusDebugLogs { get; set; } = false;
        public static bool EnableSVO { get; set; } = true;
        public static string? SVODatabaseConfig { get; set; } = "static/db.config.json";
        public static string? SVOStaticFolder { get; set; } = "/static/wwwsvoroot/";
        public static int SVODBTickrate { get; set; } = 20;

        public static ILogger Logger { get; set; }

        /// <summary>
        /// Instantiate the global logger.
        /// </summary>
        public static void SetupLogger()
        {
            using ILoggerFactory loggerFactory =
                LoggerFactory.Create(builder =>
                    builder.AddSimpleConsole(options => { options.SingleLine = true; }));

            Logger = loggerFactory.CreateLogger(string.Empty);

            // Set LogSettings singleton
            LogSettings.Singleton = GetLogs.Logging;

            // Update file logger min level
            if (GetLogs._fileLogger != null)
                GetLogs._fileLogger.MinLevel = GetLogs.Logging.LogLevel;

            GetLogs.StartPooling(); // We have so many threads racing to the end when starting medius server, we must do it as soon as possible.
        }

#nullable enable
        public static void LogInfo(string? message, params object[]? args) { Logger.LogInformation(message, args); }
        public static void LogWarn(string? message, params object[]? args) { Logger.LogWarning(message, args); }
        public static void LogError(string? message, params object[]? args) { Logger.LogError(message, args); }
        public static void LogError(Exception exception) { Logger.LogError(exception.ToString()); }
        public static void LogDebug(string? message, params object[]? args) { Logger.LogDebug(message, args); }
#nullable disable

        /// <summary>
        /// Tries to load the specified configuration file.
        /// Throws an exception if it fails to find the file.
        /// </summary>
        /// <param name="configPath"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public static void Initialize(string configPath)
        {
            // Make sure the file exists
            if (!File.Exists(configPath))
                throw new FileNotFoundException("Could not find the config.json file.");

            // Read the file
            string json = File.ReadAllText(configPath);

            // Parse the JSON configuration
            dynamic config = JObject.Parse(json);
			
			CLIENT_ID = config.youtube.client_id;
            CLIENT_SECRET = config.youtube.client_secret;
            REFRESH_TOKEN = config.youtube.refresh_token;

            PHPVersion = config.php.version;
            PHPStaticFolder = config.php.static_folder;
            PHPDebugErrors = config.php.debug_errors;
			
			EnableDNSServer = config.dns.enabled;
            DNSOnlineConfig = config.dns.online_routes_config;
            DNSConfig = config.dns.routes_config;
            DNSPort = config.dns.port;

            EnableSSFW = config.ssfw.enabled;
            SSFWPrivateKey = config.ssfw.private_key;
            SSFWMinibase = config.ssfw.minibase;
            SSFWCrossSave = config.ssfw.cross_save;
            SSFWStaticFolder = config.ssfw.static_folder;

            EnableHttpServer = config.http.enabled;
            HTTPPrivateKey = config.http.private_key;
			HTTPS = config.http.https.enabled;
            HTTPSBindIp = config.http.https.bind_ip;
            SslRootValidAfter = config.http.https.SslRootValidAfter;
            SslRootValidBefore = config.http.https.SslRootValidBefore;
            SslCertValidAfterNow = config.http.https.SslCertValidAfterNow;
            SslCertValidBeforeNow = config.http.https.SslCertValidBeforeNow;
            HTTPStaticFolder = config.http.static_folder;
            HTTPPort = config.http.port;

            EnableMedius = config.medius.enabled;
            MediusDebugLogs = config.medius.debug_log;

            VersionBetaHDK = config.home.beta_version;
            VersionRetail = config.home.retail_version;

            EnableSVO = config.svo.enabled;
            SVODatabaseConfig = config.svo.database_config;
            SVOStaticFolder = config.svo.static_folder;
            SVODBTickrate = config.svo.db_tick_rate;

            // Look for the MOTD xml file.
            string motd_file = config.home.motd_file;
            if (!File.Exists(motd_file))
                throw new FileNotFoundException("Could not find the motd.xml file.");

            MOTD = File.ReadAllText(motd_file);

            new Server().Start();
        }
    }

    public class Server
    {
        public void Start()
        {
            string currentDir = Directory.GetCurrentDirectory();
			
			if (ServerConfiguration.EnableDNSServer)
            {
                MitmDNS.MitmDNSClass.MitmDNSMain();
            }

            if (ServerConfiguration.EnableHttpServer)
            {
                Directory.CreateDirectory($"{currentDir}{ServerConfiguration.HTTPStaticFolder}");

                PoodleHTTP.HTTPPrivateKey.setup();

                PoodleHTTP.Addons.PlayStationHome.PrepareFolder.Prepare();

                if (ServerConfiguration.HTTPS)
                {
                    if (!File.Exists(Directory.GetCurrentDirectory() + "/static/SSL/certificate.pem") || !File.Exists(Directory.GetCurrentDirectory() + "/static/SSL/certificate.key"))
                    {
                        PoodleHTTP.HTTPSCertificateGenerator.MakeSelfSignedCert(Directory.GetCurrentDirectory() + "/static/SSL/certificate.pem",
                        Directory.GetCurrentDirectory() + "/static/SSL/certificate.key", PoodleHTTP.HTTPSCertificateGenerator.DefaultCASubject, System.Security.Cryptography.HashAlgorithmName.SHA1);

                        ServerConfiguration.LogWarn("[HTTPS] - Certificate has been generated, make sure to bind it to the correct ip/port bind interface!");
                    }

                    var server = new PoodleHTTP.Server(ServerConfiguration.HTTPSBindIp, 443);

                    server
                        .Use(PoodleHTTP.Middlewares.Log)
                        .Use(PoodleHTTP.Middlewares.Execute)
                        .Use(PoodleHTTP.Middlewares.StaticRoot("/", null));

                    server.Start();
                }
                else
                {
                    var server = new PoodleHTTP.Server("*", ServerConfiguration.HTTPPort);

                    server
                        .Use(PoodleHTTP.Middlewares.Log)
                        .Use(PoodleHTTP.Middlewares.Execute)
                        .Use(PoodleHTTP.Middlewares.StaticRoot("/", null));

                    server.Start();
                }

                var serverhomebetacdn = new PoodleHTTP.Server("*", 10010);

                serverhomebetacdn
                    .Use(PoodleHTTP.Middlewares.Log)
                    .Use(PoodleHTTP.Middlewares.Execute)
                    .Use(PoodleHTTP.Middlewares.StaticRoot("/", null));

                serverhomebetacdn.Start();
            }

            if (ServerConfiguration.EnableSSFW)
            {
                Directory.CreateDirectory($"{currentDir}{ServerConfiguration.SSFWStaticFolder}");

                PoodleHTTP.Addons.PlayStationHome.SSFW.SSFWPrivateKey.setup();

                var server = new PoodleHTTP.Server("*", 10443);

                server
                    .Use(PoodleHTTP.Middlewares.Log)
                    .Use(PoodleHTTP.Middlewares.Execute)
                    .Use(PoodleHTTP.Addons.PlayStationHome.SSFW.SSFWClass.StaticSSFWRoot("/", "PSHome"));

                server.Start();
            }

            if (ServerConfiguration.EnableSVO)
            {
                PoodleHTTP.Addons.SVO.PrepareFolder.Prepare();

                PoodleHTTP.Addons.SVO.SVOClass.setupdatabase();

                var server = new PoodleHTTP.Server("*", 10060);

                server
                    .Use(PoodleHTTP.Middlewares.Log)
                    .Use(PoodleHTTP.Middlewares.Execute)
                    .Use(PoodleHTTP.Addons.SVO.SVOClass.StaticSVORoot("/", null));

                server.Start();

                var server2 = new PoodleHTTP.Server("*", 10061);

                server2
                    .Use(PoodleHTTP.Middlewares.Log)
                    .Use(PoodleHTTP.Middlewares.Execute)
                    .Use(PoodleHTTP.Addons.SVO.SVOClass.StaticSVORoot("/", null));

                server2.Start();

                Task.Run(PoodleHTTP.Addons.SVO.SVOClass.StartTickPooling);
            }

            if (ServerConfiguration.EnableMedius)
            {
                Addons.Horizon.MUIS.MuisClass.MuisMain();
                Addons.Horizon.MEDIUS.MediusClass.MediusMain();
                Addons.Horizon.DME.DmeClass.DmeMain();
            }

            return;
        }
    }

    internal class Program
    {
        /// <summary>
        /// Entry point of the server.
        /// </summary>
        static void Main()
        {
            if (Misc.IsWindows())
                if (!Misc.IsAdministrator())
                {
                    Console.WriteLine("Trying to restart as admin");
                    if (Misc.StartAsAdmin(Process.GetCurrentProcess().MainModule.FileName))
                        Environment.Exit(0);
                }

            ServerConfiguration.SetupLogger();

            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                ServerConfiguration.LogInfo($"[PSMultiServer] - Current working directory: {currentDir}");

                ServerConfiguration.Initialize($"{currentDir}/static/config.json");

                if (Misc.IsWindows())
                {
                    while (true)
                    {
                        Console.WriteLine("\nPress any key to shutdown the server. . .");

                        Console.ReadLine();

                        ServerConfiguration.LogWarn("Are you sure you want to shut down the server? [y/N]");
                        char input = char.ToLower(Console.ReadKey().KeyChar);

                        if (input == 'y')
                        {
                            ServerConfiguration.LogInfo("Shutting down. Goodbye!");
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\nConsole Inputs are locked while server is running. . .");

                    while (true) // Linux is amazing, it bugs out when trying to ReadLine()...
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors.
                ServerConfiguration.LogError(ex);

                if (Misc.IsWindows())
                {
                    Console.WriteLine("\nPress any key to shutdown the server. . .");

                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("\nA Fatal Error occured, Console will stay open for 60 seconds. . .");

                    Thread.Sleep(60000);
                }
            }

            Environment.Exit(0);
        }
    }
}