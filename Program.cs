using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace PSMultiServer
{
    public class ServerConfiguration
    {
        public string? PHPVER { get; set; }
        public string? SSFWKEY { get; set; }
        public string? HTTPKEY { get; set; }
        public string? SSFWminibase { get; set; }
        public int HTTPPort { get; set; }
        public bool EnableHTTPserver { get; set; }
        public bool EnableSSFW { get; set; }
        public bool EnableSVO { get; set; }
        public bool SSFWcrosssave { get; set; }
    }

    public class Server
    {
        private readonly string _PHPVER;
        private readonly string _SSFWKEY;
        private readonly string _HTTPKEY;
        private readonly string _SSFWminibase;
        private readonly int _HTTPPort;
        private readonly bool _enableHTTPserver;
        private readonly bool _enableSSFW;
        private readonly bool _enableSVO;
        private readonly bool _SSFWcrosssave;

        public Server(ServerConfiguration config)
        {
            _PHPVER = config.PHPVER;
            _SSFWKEY = config.SSFWKEY;
            _HTTPKEY = config.HTTPKEY;
            _SSFWminibase = config.SSFWminibase;
            _HTTPPort = config.HTTPPort;
            _enableHTTPserver = config.EnableHTTPserver;
            _enableSSFW = config.EnableSSFW;
            _enableSVO = config.EnableSVO;
            _SSFWcrosssave = config.SSFWcrosssave;
        }
        public void Start()
        {
            if (_enableHTTPserver && HTTPserver.httpstarted == false)
            {
                HTTPserver.HTTPstart(_PHPVER, _HTTPKEY, _HTTPPort);
            }

            if (_enableSSFW && SRC_Addons.HOME.SSFWServices.ssfwstarted == false)
            {
                SRC_Addons.HOME.SSFWServices.SSFWstart(_SSFWKEY, _SSFWminibase, _SSFWcrosssave);
            }

            if (_enableSVO && SRC_Addons.MEDIUS.SVO.SVO_OTG.svostarted == false)
            {
                SRC_Addons.MEDIUS.SVO.SVO_OTG.SVOstart();
            }

            return;
        }
        public void Stop()
        {
            if (HTTPserver.httpstarted)
            {
                Console.WriteLine("HTTP Server stopped");

                HTTPserver.HTTPstop();
            }

            if (SRC_Addons.HOME.SSFWServices.ssfwstarted)
            {
                Console.WriteLine("SSFW Server stopped");

                SRC_Addons.HOME.SSFWServices.SSFWstop();
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
                    File.WriteAllText(Directory.GetCurrentDirectory() + @"/config.json", "{\r\n  \"PHPVER\": \"8.25\",\r\n  \"SSFWKEY\": \"\",\r\n  \"HTTPKEY\": \"\",\r\n  \"SSFWminibase\": \"[]\",\r\n  \"HTTPPort\": 80,\r\n  \"EnableHTTPserver\": true,\r\n  \"EnableSSFW\": true,\r\n  \"EnableSVO\": true,\r\n  \"SSFWcrosssave\": true\r\n}");
                }

                // Read the configuration file
                string configFile = "config.json";
                string json = File.ReadAllText(configFile);

                // Parse the JSON configuration
                var config = JsonConvert.DeserializeObject<ServerConfiguration>(json);

                // Create and start the server
                var server = new Server(config);

                server.Start();

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
                            server.Stop();

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