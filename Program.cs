using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using NReco.Logging.File;
using MultiServer.HTTPService;
using MultiServer.PluginManager;
using MultiServer.Addons.Horizon.DME;
using MultiServer.Addons.Horizon.MEDIUS;
using MultiServer.Addons.Horizon.MUIS;
using MultiServer.Addons.Horizon.LIBRARY.Database;
using Newtonsoft.Json;

namespace MultiServer
{
    public static class ServerConfiguration
    {
#nullable enable
        public static string ConsoleMode { get; set; } = "";
        public static string PluginsFolder { get; set; } = "/static/plugins/";
        public static string PHPVersion { get; set; } = "8.25";
        public static string PHPStaticFolder { get; set; } = "/static/PHP/";
        public static bool PHPDebugErrors { get; set; } = false;
		public static string DNSConfig { get; set; } = "/static/routes.txt";
        public static string DNSOnlineConfig { get; set; } = "";
        public static int DNSPort { get; set; } = 53;
        public static string? SSFWPrivateKey { get; set; }
        public static bool SSFWCrossSave { get; set; } = true;
        public static string? SSFWStaticFolder { get; set; } = "/static/wwwssfwroot/";
        public static string? HTTPPrivateKey { get; set; }
        public static int HTTPPort { get; set; } = 80;
        public static string? HTTPStaticFolder { get; set; } = "/static/wwwroot/";
        public static string? HomeToolsHelperStaticFolder { get; set; } = "/static/XmlHelper/";
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
        public static bool EnableHomeTools { get; set; } = true;
        public static bool EnableUpscale { get; set; } = true;
        public static bool EnableSSFW { get; set; } = true;
        public static bool EnableMedius { get; set; } = true;
        public static bool MediusDebugLogs { get; set; } = false;
        public static bool EnableSVO { get; set; } = true;
        public static string? DMEConfig { get; set; } = "/static/medius.json";
        public static string? MEDIUSConfig { get; set; } = "/static/muis.json";
        public static string? MUISConfig { get; set; } = "/static/muis.json";
        public static string? DatabaseConfig { get; set; } = "static/medius.db.config.json";
        public static string? SVOStaticFolder { get; set; } = "/static/wwwsvoroot/";

        public static IpsData? IpsData; // Global variable to store banned IPs

        public static HttpsDns? HttpsDns; // Global variable to store HTTPS DNS list.
#nullable disable
        public static DbController Database = null;
        public static ILogger Logger { get; set; }

        public static FileLoggerProvider _fileLogger = null;

        /// <summary>
        /// Instantiate the global logger.
        /// </summary>
        public static void SetupServer()
        {
            RefreshVariables($"{Directory.GetCurrentDirectory()}/static/config.json");

            if (ConsoleMode.ToLower() == "json")
            {
                var loggingOptions = new FileLoggerOptions()
                {
                    Append = false,
                    FileSizeLimitBytes = 4096 * 4096 * 1,
                    MaxRollingFiles = 100,
                    FormatLogEntry = (msg) => {
                        var sb = new System.Text.StringBuilder();
                        StringWriter sw = new StringWriter(sb);
                        var jsonWriter = new JsonTextWriter(sw);
                        jsonWriter.WriteStartArray();
                        jsonWriter.WriteValue(DateTime.Now.ToString("o"));
                        jsonWriter.WriteValue(msg.LogLevel.ToString());
                        jsonWriter.WriteValue(msg.LogName);
                        jsonWriter.WriteValue(msg.EventId.Id);
                        jsonWriter.WriteValue(msg.Message);
                        jsonWriter.WriteValue(msg.Exception?.ToString());
                        jsonWriter.WriteEndArray();
                        return sb.ToString();
                    }
                };

                using ILoggerFactory loggerFactory =
                    LoggerFactory.Create(builder =>
                    {
                        builder.AddJsonConsole();

                        builder.AddProvider(_fileLogger = new FileLoggerProvider(Directory.GetCurrentDirectory() + "/MultiServer_log.json", loggingOptions));
                        _fileLogger.MinLevel = LogLevel.Information;
                    });

                Logger = loggerFactory.CreateLogger(string.Empty);
            }
            else
            {
                var loggingOptions = new FileLoggerOptions()
                {
                    Append = false,
                    FileSizeLimitBytes = 2147483648, // 2 GB in bytes
                    MaxRollingFiles = 100
                };

                using ILoggerFactory loggerFactory =
                    LoggerFactory.Create(builder =>
                    {
                        if (ConsoleMode.ToLower() == "systemd")
                            builder.AddSystemdConsole();
                        else
                            builder.AddSimpleConsole(options => { options.SingleLine = true; });

                        builder.AddProvider(_fileLogger = new FileLoggerProvider(Directory.GetCurrentDirectory() + "/MultiServer.log", loggingOptions));
                        _fileLogger.MinLevel = LogLevel.Information;
                    });

                Logger = loggerFactory.CreateLogger(string.Empty);
            }

            new Server().Start();
        }

#pragma warning disable
        public static void LogInfo(string? message, params object[]? args) { Logger.LogInformation(message, args); }
        public static void LogWarn(string? message, params object[]? args) { Logger.LogWarning(message, args); }
        public static void LogError(string? message, params object[]? args) { Logger.LogError(message, args); }
        public static void LogError(Exception exception) { Logger.LogError(exception.ToString()); }
        public static void LogDebug(string? message, params object[]? args) { Logger.LogDebug(message, args); }
#pragma warning restore

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
                throw new FileNotFoundException("Could not find the config.json file.");

            // Read the file
            string json = File.ReadAllText(configPath);

            // Parse the JSON configuration
            dynamic config = JObject.Parse(json);

            ConsoleMode = config.logging_mode;

            PluginsFolder = config.plugins_folder;

            PHPVersion = config.php.version;

            if (Misc.IsWindows())
            {
                if (Environment.Is64BitOperatingSystem)
                    PHPStaticFolder = config.php.static_folder + "Windows_x64/";
                else
                    PHPStaticFolder = config.php.static_folder + "Windows/";
            }
            else
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
            EnableHomeTools = config.http.home_tools;
            EnableUpscale = config.http.upscale;
            HTTPPrivateKey = config.http.private_key;
            HTTPStaticFolder = config.http.static_folder;
            HomeToolsHelperStaticFolder = config.http.hometools_helper_static_folder;
            HTTPPort = config.http.port;
			
            EnableMedius = config.medius.enabled;
            DMEConfig = config.medius.dme_config;
            MEDIUSConfig = config.medius.medius_config;
            MUISConfig = config.medius.muis_config;
            MediusDebugLogs = config.medius.debug_log;

            DatabaseConfig = config.medius_database.database_config;

            VersionBetaHDK = config.home.beta_version;
            VersionRetail = config.home.retail_version;

            EnableSVO = config.svo.enabled;
            SVOStaticFolder = config.svo.static_folder;

            // Look for the MOTD xml file.
            string motd_file = config.home.motd_file;
            if (!File.Exists(motd_file))
                throw new FileNotFoundException("Could not find the motd.xml file.");

            MOTD = File.ReadAllText(motd_file);

            LoadIPs();

            LoadDNSs();
        }

        private static void LoadIPs()
        {
            try
            {
                IpsData = JsonConvert.DeserializeObject<IpsData>(File.ReadAllText($"{Directory.GetCurrentDirectory()}/static/config.json"));
            }
            catch (Exception ex)
            {
                LogError($"Error loading IPs: {ex}");
            }
        }

        private static void LoadDNSs()
        {
            try
            {
                HttpsDns = JsonConvert.DeserializeObject<HttpsDns>(File.ReadAllText($"{Directory.GetCurrentDirectory()}/static/config.json"));
            }
            catch (Exception ex)
            {
                LogError($"Error loading HttpsDns entries: {ex}");
            }
        }

        public static bool IsIPBanned(string ipAddress)
        {
            try
            {
                return IpsData?.BannedIPs.Contains(ipAddress) ?? false;
            }
            catch (Exception ex)
            {
                LogError($"Error loading Banned IPs: {ex}");
            }

            return true;
        }

        public static bool IsIPAllowed(string ipAddress)
        {
            try
            {
                return IpsData?.HomeToolsAllowedIPs.Contains(ipAddress) ?? false;
            }
            catch (Exception ex)
            {
                LogError($"Error loading Allowed IPs: {ex}");
            }

            return false;
        }

        public static Task RefreshConfig()
        {
            while (true)
            {
                // Sleep for 5 minutes (300,000 milliseconds)
                Thread.Sleep(5 * 60 * 1000);

                // Your task logic goes here
                LogInfo("Config Refresh at - " + DateTime.Now);

                RefreshVariables($"{Directory.GetCurrentDirectory()}/static/config.json");
            }
        }
    }

    public class IpsData
    {
        public List<string> BannedIPs { get; set; }
        public List<string> HomeToolsAllowedIPs { get; set; }
    }

    public class HttpsDns
    {
        public List<string> HttpsDnsList { get; set; }
    }

    public class Server
    {
        public static string pluginspath = Directory.GetCurrentDirectory() + ServerConfiguration.PluginsFolder;

        public static List<IPlugin> plugins = PluginLoader.LoadPluginsFromFolder(pluginspath);

        private static void setupdatabase()
        {
            ServerConfiguration.Database = new(Directory.GetCurrentDirectory() + $"/{ServerConfiguration.DatabaseConfig}");
        }

        public Task HorizonStarter()
        {
            if (ServerConfiguration.EnableMedius)
            {
                Parallel.Invoke(
                   async () => await MediusClass.MediusMain(),
                   async () => await MuisClass.MuisMain(),
                   async () => await DmeClass.DmeMain()
               );
            }

            return Task.CompletedTask;
        }

        public Task ServerStarter()
        {
            if (ServerConfiguration.EnableDNSServer)
                MitmDNS.MitmDNSClass.MitmDNSMain();

            string currentDir = Directory.GetCurrentDirectory();

            Directory.CreateDirectory(currentDir + "/static/SSL");

            if (!File.Exists(currentDir + "/static/SSL/MultiServer.pfx"))
                CertificateUtils.CreateHomeCertificatesFile(CertificateUtils.CreateSelfSignedCert(currentDir + "/static/SSL/MultiServer.pfx", "secure.cprod.homeps3.online.scee.com"), currentDir + "/static/CERTIFICATES.TXT");

            if (!File.Exists(currentDir + "/static/SSL/SVO.pfx"))
                CertificateUtils.CreateLegacySelfSignedCert(currentDir + "/static/SSL/SVO.pfx", "root.com");

            if (Misc.IsWindows())
                if (!Misc.IsAdministrator())
                    return Task.CompletedTask;

            if (ServerConfiguration.EnableHttpServer)
            {
                Directory.CreateDirectory($"{currentDir}{ServerConfiguration.HTTPStaticFolder}");

                if (File.Exists(currentDir + "/static/SSL/MultiServer.pfx"))
                    Task.Run(async () => await HTTPClass.HTTPstart(ServerConfiguration.HTTPPort, true));
                else
                    Task.Run(async () => await HTTPClass.HTTPstart(ServerConfiguration.HTTPPort, false));
            }

            if (ServerConfiguration.EnableSSFW)
            {
                Directory.CreateDirectory($"{currentDir}{ServerConfiguration.SSFWStaticFolder}");
                Task.Run(HTTPService.Addons.PlayStationHome.SSFW.SSFWClass.SSFWstart);
            }

            if (ServerConfiguration.EnableSVO)
            {
                if (File.Exists(currentDir + "/static/SSL/MultiServer.pfx"))
                    Parallel.Invoke(
                       async () => await HTTPService.Addons.SVO.SVOClass.SVOstart(true),
                       async () => await HTTPService.Addons.SVO.SVOClass.StartTickPooling()
                    );
                else
                    Parallel.Invoke(
                       async () => await HTTPService.Addons.SVO.SVOClass.SVOstart(false),
                       async () => await HTTPService.Addons.SVO.SVOClass.StartTickPooling()
                    );
            }

            return Task.CompletedTask;
        }

        public void Start()
        {
            setupdatabase();

            Parallel.Invoke(
                   async () => await ServerStarter(),
                   async () => await HorizonStarter()
               );
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

            try
            {
                Console.WriteLine($"Current working directory: {Directory.GetCurrentDirectory()}");

                ServerConfiguration.SetupServer();

                _ = Task.Run(ServerConfiguration.RefreshConfig);

                if (Misc.IsWindows())
                {
                    while (true)
                    {
                        ServerConfiguration.LogInfo("Press any key to shutdown the server. . .");

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
                    ServerConfiguration.LogWarn("\nConsole Inputs are locked while server is running. . .");

                    Thread.Sleep(Timeout.Infinite); // While-true on Linux are thread blocking if on main static.
                }
            }
            catch (Exception ex)
            {
                // Handle any errors using writeline,
                // as the server init can also fail, thus, causing our logger to be null.
                Console.WriteLine($"\n{ex}");

                if (Misc.IsWindows())
                {
                    Console.WriteLine("\nA Fatal Error occured, press any key to shutdown the server. . .");

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