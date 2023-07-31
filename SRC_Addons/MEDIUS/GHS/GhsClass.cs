using DotNetty.Common.Internal.Logging;
using PSMultiServer.Addons.Medius.Server.Common;
using PSMultiServer.Addons.Medius.Server.Database;
using PSMultiServer.Addons.Medius.GHS.Config;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Newtonsoft.Json;
using System.Management;

namespace PSMultiServer.Addons.Medius.GHS
{
    public class GhsClass
    {
        private static string CONFIG_DIRECTIORY = "./loginformNtemplates/GHS";

        public static string CONFIG_FILE = Path.Combine(CONFIG_DIRECTIORY, "ghs.json");
        public static string DB_CONFIG_FILE = Path.Combine(CONFIG_DIRECTIORY, "db.config.json");

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<GhsClass>();

        public static ServerSettings Settings = new ServerSettings();
        public static DbController Database = new DbController(DB_CONFIG_FILE);

        public static GHS GHSServer = new GHS(Settings.Port);

        static async Task StartServerAsync()
        {
            DateTime lastConfigRefresh = Utils.GetHighPrecisionUtcTime();

            string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");

            ServerConfiguration.LogInfo("**************************************************");
            #region MediusGetBuildTimeStamp
            var MediusBuildTimeStamp = GetLinkerTime(Assembly.GetEntryAssembly());
            ServerConfiguration.LogInfo($"* MediusBuildTimeStamp at {MediusBuildTimeStamp}");
            #endregion

            string gpszVersionString = "1.0.0";

            ServerConfiguration.LogInfo($"* Global Handle Service Version {gpszVersionString}");
            ServerConfiguration.LogInfo($"* Launched on {datetime}");

            await GHSServer.Start();

            //* Process ID: %d , Parent Process ID: %d
            if (Database._settings.SimulatedMode == true)
            {
                ServerConfiguration.LogInfo("* Database Disabled GHS");
            }
            else
            {
                ServerConfiguration.LogInfo("* Database Enabled GHS");
            }

            ServerConfiguration.LogInfo($"* Server Key Type: {Settings.EncryptMessages}");

            #region Remote Log Viewing
            if (Settings.RemoteLogViewPort == 0)
            {
                //* Remote log viewing setup failure with port %d.
                ServerConfiguration.LogInfo("* Remote log viewing disabled.");
            }
            else if (Settings.RemoteLogViewPort != 0)
            {
                ServerConfiguration.LogInfo($"* Remote log viewing enabled at port {Settings.RemoteLogViewPort}.");
            }
            #endregion

            //* Diagnostic Profiling Enabled: %d Counts

            ServerConfiguration.LogInfo("**************************************************");

            if (Settings.NATIp != null)
            {
                IPAddress ip = IPAddress.Parse(Settings.NATIp);
                DoGetHostEntry(ip);
            }

            ServerConfiguration.LogInfo($"GHS started.");


            try
            {
                while (true)
                {
                    // Tick
                    await Task.WhenAll(GHSServer.Tick());

                    // Reload config
                    if ((Utils.GetHighPrecisionUtcTime() - lastConfigRefresh).TotalMilliseconds > Settings.RefreshConfigInterval)
                    {
                        RefreshConfig();
                        lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
                    }

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError(ex);
            }
            finally
            {

            }
        }

        public static async Task GhsMain()
        {
            // 
            Initialize();

            // 
            await StartServerAsync();
        }

        static void Initialize()
        {
            RefreshConfig();
        }

        /// <summary>
        /// 
        /// </summary>
        static void RefreshConfig()
        {
            // 
            var serializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            // Load settings
            if (File.Exists(CONFIG_FILE))
            {
                // Populate existing object
                JsonConvert.PopulateObject(File.ReadAllText(CONFIG_FILE), Settings, serializerSettings);
            }
            else
            {
                // Save defaults
                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }

            // Update default rsa key
            Server.Pipeline.Attribute.ScertClientAttribute.DefaultRsaAuthKey = Settings.DefaultKey;
        }

        public static void DoGetHostEntry(IPAddress address)
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry(address);

                ServerConfiguration.LogInfo($"* NAT Service IP: {address}");
                //ServerConfiguration.LogInfo($"GetHostEntry({address}) returns HostName: {host.HostName}");
            }
            catch (SocketException ex)
            {
                //unknown host or
                //not every IP has a name
                //log exception (manage it)
                ServerConfiguration.LogError($"* NAT not resolved {ex}");
            }
        }

        #region System Time
        public static DateTime GetLinkerTime(Assembly assembly)
        {
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if (index > 0)
                {
                    value = value[(index + BuildVersionMetadataPrefix.Length)..];
                    return DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:ss:fffZ", CultureInfo.InvariantCulture);
                }
            }

            return default;
        }

        public static TimeSpan GetUptime()
        {
            ManagementObject mo = new(@"\\.\root\cimv2:Win32_OperatingSystem=@");
            DateTime lastBootUp = ManagementDateTimeConverter.ToDateTime(mo["LastBootUpTime"].ToString());
            return DateTime.Now.ToUniversalTime() - lastBootUp.ToUniversalTime();
        }
        #endregion
    }
}
