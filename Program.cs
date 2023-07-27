using Newtonsoft.Json;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Logging;
using PSMultiServer.SRC_Addons.MEDIUS.SVO;
using System.Net.NetworkInformation;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace PSMultiServer
{
    public class ServerConfiguration
    {
        public ServerConfiguration()
        {
            PHPVER = "8.25";
            SSFWKEY = "";
            HTTPKEY = "";
            SSFWminibase = "[]";
            HOMEversion_BETA_HDK = "01.60";
            HOMEversion_RETAIL_HDKONLINEDEBUG = "01.83";
            HOMEmessageoftheday = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
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
            HTTPPort = 80;
            EnableHTTPserver = true;
            EnableSSFW = true;
            EnableMEDIUS = true;
            SSFWcrosssave = false;
        }

        public string? PHPVER { get; set; }
        public string? SSFWKEY { get; set; }
        public string? HTTPKEY { get; set; }
        public string? SSFWminibase { get; set; }
        public string? HOMEversion_BETA_HDK { get; set; }
        public string? HOMEversion_RETAIL_HDKONLINEDEBUG { get; set; }
        public string? HOMEmessageoftheday { get; set; }
        public int HTTPPort { get; set; }
        public bool EnableHTTPserver { get; set; }
        public bool EnableSSFW { get; set; }
        public bool EnableMEDIUS { get; set; }
        public bool SSFWcrosssave { get; set; }
    }

    public class Server
    {
        private readonly string _PHPVER;
        private readonly string _SSFWKEY;
        private readonly string _HTTPKEY;
        private readonly string _SSFWminibase;
        private readonly string _HOMEversion_BETA_HDK;
        private readonly string _HOMEversion_RETAIL_HDKONLINEDEBUG;
        private readonly string _HOMEmessageoftheday;
        private readonly int _HTTPPort;
        private readonly bool _enableHTTPserver;
        private readonly bool _enableSSFW;
        private readonly bool _enableMEDIUS;
        private readonly bool _SSFWcrosssave;

        public Server(ServerConfiguration config)
        {
            _PHPVER = config.PHPVER;
            _SSFWKEY = config.SSFWKEY;
            _HTTPKEY = config.HTTPKEY;
            _SSFWminibase = config.SSFWminibase;
            _HOMEversion_BETA_HDK = config.HOMEversion_BETA_HDK;
            _HOMEversion_RETAIL_HDKONLINEDEBUG = config.HOMEversion_RETAIL_HDKONLINEDEBUG;
            _HOMEmessageoftheday = config.HOMEmessageoftheday;
            _HTTPPort = config.HTTPPort;
            _enableHTTPserver = config.EnableHTTPserver;
            _enableSSFW = config.EnableSSFW;
            _enableMEDIUS = config.EnableMEDIUS;
            _SSFWcrosssave = config.SSFWcrosssave;
        }

        private static bool IsSupportedArchive(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return (extension == ".zip" || extension == ".rar" || extension == ".tar" ||
                    extension == ".gz" || extension == ".7z");
        }

        public async Task ExtractToHTTPFolder(string sourceFolder, string destinationFolder)
        {
            // Check if the source folder exists
            if (!Directory.Exists(sourceFolder))
            {
                Console.WriteLine("HTTP : Source folder does not exist.");
                return;
            }

            // Check if the destination folder exists
            if (!Directory.Exists(destinationFolder))
            {
                Console.WriteLine("HTTP : Destination folder does not exist.");
                return;
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

            return;
        }

        public void Start()
        {
            if (_enableHTTPserver && HTTPserver.httpstarted == false)
            {
                if (!Directory.Exists(Directory.GetCurrentDirectory() + @"/wwwroot/"))
                {
                    Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"/wwwroot/");
                }

                if (Directory.Exists(Directory.GetCurrentDirectory() + @"/HTTPServer_Archives/"))
                {
                    Task.Run(() => ExtractToHTTPFolder(Directory.GetCurrentDirectory() + "/HTTPServer_Archives/", Directory.GetCurrentDirectory() + "/wwwroot/"));
                }

                HTTPserver.HTTPstart(_PHPVER, _HTTPKEY, _HTTPPort);
            }

            if (_enableSSFW && SRC_Addons.HOME.SSFWServices.ssfwstarted == false)
            {
                SRC_Addons.HOME.SSFWServices.SSFWstart(_SSFWKEY, _SSFWminibase, _SSFWcrosssave);
            }

            if (_enableMEDIUS)
            {
                // Set LogSettings singleton
                LogSettings.Singleton = GetLogs.Logging;

                // Update file logger min level
                if (GetLogs._fileLogger != null)
                    GetLogs._fileLogger.MinLevel = GetLogs.Logging.LogLevel;

                GetLogs.StartPooling(); // We have so many threads racing to the end when starting medius server, we must do it as soon as possible.

                if (SVO.svostarted == false)
                {
                    Task.Run(() => SvoClass.SvoMain(_HOMEmessageoftheday));
                }

                Task.Run(() => SRC_Addons.MEDIUS.MEDIUS.MediusClass.MediusMain());

                Task.Run(() => SRC_Addons.MEDIUS.BWPS.BwpsClass.BwpsMain());

                Task.Run(() => SRC_Addons.MEDIUS.GHS.GhsClass.GhsMain());

                Task.Run(() => SRC_Addons.MEDIUS.MUIS.MuisClass.MuisMain(_HOMEversion_BETA_HDK, _HOMEversion_RETAIL_HDKONLINEDEBUG));

                Task.Run(() => SRC_Addons.MEDIUS.DME.DmeClass.DmeMain());
            }

            return;
        }
    }
    internal class Program
    {
        static void Main()
        {
            try
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + @"/config.json"))
                {
                    ServerConfiguration configuration = new ServerConfiguration();

                    string json = JsonConvert.SerializeObject(configuration, Formatting.Indented);

                    File.WriteAllText(Directory.GetCurrentDirectory() + @"/config.json", json);

                    // Create and start the server
                    var server = new Server(configuration);

                    server.Start();
                }
                else
                {
                    // Read the configuration file
                    string configFile = "config.json";
                    string json = File.ReadAllText(configFile);

                    // Parse the JSON configuration
                    var config = JsonConvert.DeserializeObject<ServerConfiguration>(json);

                    // Create and start the server
                    var server = new Server(config);

                    server.Start();
                }

                while (true)
                {
                    Console.WriteLine("Press any key to shutdown the server...");

                    Console.ReadLine();

                    Console.WriteLine("Are you sure you want to SHUTDOWN THE SERVER? Press no if you don't want to!");

                    string emergencyin = Console.ReadLine().ToLower();

                    if (emergencyin == "no" || emergencyin == "n0")
                    {

                    }
                    else
                    {
                        Console.WriteLine("Are you REALLY sure you want to SHUTDOWN THE SERVER? Press no if you don't want to!");

                        emergencyin = Console.ReadLine().ToLower();

                        if (emergencyin == "no" || emergencyin == "n0")
                        {

                        }
                        else
                        {
                            Environment.Exit(0);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
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