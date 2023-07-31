using PSMultiServer.Addons.Medius.Server.Common.Logging;
using PSMultiServer.Addons.Medius.SVO;
using System.Net.NetworkInformation;
using SharpCompress.Archives;
using SharpCompress.Common;
using PSMultiServer.Addons.Medius.MEDIUS;
using PSMultiServer.Addons.Medius.MUIS;
using PSMultiServer.Addons.Medius.GHS;
using PSMultiServer.Addons.Medius.BWPS;
using PSMultiServer.Addons.Medius.DME;
using Newtonsoft.Json.Linq;

namespace PSMultiServer
{
    public static class ServerConfiguration
    {
        public static string PHPVersion { get; set; } = "8.25";
        public static string? SSFWPrivateKey { get; set; }
        public static bool SSFWCrossSave { get; set; } = true;
        public static string? HTTPPrivateKey { get; set; }
        public static int HTTPPort { get; set; } = 80;
        public static string SSFWMinibase { get; set; } = "[]";
        public static string VersionBetaHDK { get; set; } = "01.60";
        public static string VersionRetail { get; set; } = "01.83";
        public static string? MOTD { get; set; } = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
            "<SVML>\r\n" +
            "    <RECTANGLE class=\"CHIP_FACE\" name=\"backPanel\" x=\"292\" y=\"140\" width=\"708\" height=\"440\"/>\r\n" +
            "    <RECTANGLE class=\"CHIP_RECESS\" name=\"backPanel\" x=\"300\" y=\"148\" width=\"692\" height=\"384\" fillColor=\"#FFFFFFFF\"/>\r\n\r\n" +
            "    <TEXT name=\"text\" x=\"640\" y=\"171\" width=\"636\" height=\"26\" fontSize=\"26\" align=\"center\" textColor=\"#cc000000\">Message Of the Day</TEXT>\r\n\r\n" +
            "    <TEXTAREA class=\"TEXTAREA1\" name=\"message\" x=\"308\" y=\"204\" width=\"664\" height=\"320\"\r\n\t\t" +
            "fontSize=\"22\" lineSpacing=\"22\" linesVisible=\"14\"\r\n\t\t" +
            "readonly=\"true\" selectable=\"false\" blinkCursor=\"false\"\r\n\t\t" +
            "textColor=\"#CC000000\" highlightTextColor=\"#FF000000\"\r\n\t\t" +
            "leftPadValue=\"8\" topPadValue=\"8\" \r\n" +
            "        defaultTextEntry=\"1\" defaultTextScroll=\"1\">Welcome to PlayStationÂ®Home Open Beta.\r\n\r\n" +
            "Head over to the new Resident Evil 5 Studio Lot space, accessible via the Menu Pad by selecting Locations &gt; World Map and then clicking on the Capcom chip. Here you can enjoy an interactive behind-the-scenes look at the tools and devices used on location for the filming of a portion of Resident Evil 5.\r\n\r\n" +
            "CydoniaX (PlayStationÂ®Home Community Manager) &amp; Locust_Star (PlayStationÂ®Home Community Specialist)</TEXTAREA>\r\n" +
            "    \r\n" +
            "    <TEXT name=\"legend\" x=\"984\" y=\"548\" width=\"652\" height=\"18\" fontSize=\"18\" align=\"right\" textColor=\"#CCFFFFFF\">[CROSS] Continue</TEXT>\r\n" +
            "    <QUICKLINK name=\"refresh\" button=\"SV_PAD_X\" linkOption=\"NORMAL\" href=\"../home/homeEnterWorld.jsp\"/>\r\n" +
            "</SVML>";
        public static bool EnableHttpServer { get; set; } = true;
        public static bool EnableSSFW { get; set; } = true;
        public static bool EnableMedius { get; set; } = true;

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

            PHPVersion = config.php.version;

            EnableSSFW = config.ssfw.enabled;
            SSFWPrivateKey = config.ssfw.private_key;
            SSFWMinibase = config.ssfw.minibase;
            SSFWCrossSave = config.ssfw.cross_save;

            EnableHttpServer = config.http.enabled;
            HTTPPrivateKey = config.http.private_key;
            HTTPPort = config.http.port;

            EnableMedius = config.medius.enabled;

            VersionBetaHDK = config.home.beta_version;
            VersionRetail = config.home.retail_version;

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
        private static bool IsSupportedArchive(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return (extension == ".zip" || extension == ".rar" || extension == ".tar" ||
                    extension == ".gz" || extension == ".7z");
        }

        public static bool ExtractToHTTPFolder(string sourceFolder, string destinationFolder)
        {
            // Check if the source folder exists
            if (!Directory.Exists(sourceFolder))
            {
                Console.WriteLine("HTTP : Source folder does not exist.");
                return false;
            }

            // Check if the destination folder exists
            if (!Directory.Exists(destinationFolder))
            {
                Console.WriteLine("HTTP : Destination folder does not exist.");
                return false;
            }

            // Get all archive files in the source folder
            string[] archiveFiles = Directory.GetFiles(sourceFolder, "*.*")
                                             .Where(file => IsSupportedArchive(file))
                                             .ToArray();

            // Extract files from each archive
            foreach (string archiveFile in archiveFiles)
            {
                string extension = Path.GetExtension(archiveFile).ToLower();
                string fileName = Path.GetFileNameWithoutExtension(archiveFile);
                string destinationPath = Path.Combine(destinationFolder, fileName);

                try
                {
                    // Check if the destination folder exists
                    if (!Directory.Exists(destinationPath))
                    {
                        Directory.CreateDirectory(destinationPath);
                    }

                    // Extract the archive based on its extension
                    using (var archive = Misc.GetArchiveReader(archiveFile))
                    {
                        if (archive != null)
                        {
                            foreach (var entry in archive.Entries)
                            {
                                if (!entry.IsDirectory)
                                {
                                    entry.WriteToDirectory(destinationPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                }
                            }
                        }
                    }

                    Console.WriteLine($"HTTP : Successfully extracted {extension.ToUpper()} file: {archiveFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"HTTP : Error extracting {extension.ToUpper()} file: {archiveFile}\n{ex.Message}");
                }
            }

            return true;
        }

        public void Start()
        {
            string currentDir = Directory.GetCurrentDirectory();

            if (ServerConfiguration.EnableHttpServer && HTTPserver.httpstarted == false)
            {
                Directory.CreateDirectory($"{currentDir}/wwwroot/");

                if (Directory.Exists($"{currentDir}/HTTPServer_Archives/"))
                    ExtractToHTTPFolder($"{currentDir}/HTTPServer_Archives/", $"{currentDir}/wwwroot/");

                HTTPserver.HTTPstart();
            }

            if (ServerConfiguration.EnableSSFW && Addons.HOME.SSFWServices.Started == false)
            {
                Addons.HOME.SSFWServices.SSFWstart();
            }

            if (ServerConfiguration.EnableMedius)
            {
                // Set LogSettings singleton
                LogSettings.Singleton = GetLogs.Logging;

                // Update file logger min level
                if (GetLogs._fileLogger != null)
                    GetLogs._fileLogger.MinLevel = GetLogs.Logging.LogLevel;

                GetLogs.StartPooling(); // We have so many threads racing to the end when starting medius server, we must do it as soon as possible.

                Parallel.Invoke(
                    async () => await SvoClass.SvoMain(),
                    async () => await MediusClass.MediusMain(),
                    async () => await BwpsClass.BwpsMain(),
                    async () => await GhsClass.GhsMain(),
                    async () => await MuisClass.MuisMain(),
                    async () => await DmeClass.DmeMain()
                );
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
            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                ServerConfiguration.Initialize($"{currentDir}static/config.json");

                while (true)
                {
                    Console.WriteLine("Press any key to shutdown the server...");
                    Console.ReadLine();

                    Console.WriteLine("Are you sure you want to shut down the server? [y/N]");
                    char input = char.ToLower(Console.ReadKey().KeyChar);

                    if (input == 'y')
                    {
                        Console.WriteLine("Shutting down. Goodbye!");
                        Environment.Exit(0);
                    }
                }
            } catch (FileNotFoundException e)
            {
                // Handle any missing file.
                Console.WriteLine(e.Message);
            }
        }

        private bool IsPortOpen(string host, int port)
        {
            TcpConnectionInformation[] tcpConnections = IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections();

            foreach (TcpConnectionInformation connection in tcpConnections)
            {
                if (connection.LocalEndPoint.Port == port && connection.RemoteEndPoint.Address.ToString() == host)
                {
                    return true;
                }
            }

            return false;
        }
    }
}