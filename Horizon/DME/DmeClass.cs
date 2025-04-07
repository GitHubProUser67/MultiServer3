using CustomLogger;
using Newtonsoft.Json;
using BackendProject.Horizon.RT.Models;
using BackendProject.Horizon.LIBRARY.Common;
using Horizon.DME.Config;
using Horizon.DME.Models;
using System.Diagnostics;
using System.Net;
using Horizon.PluginManager;
using BackendProject;
using Horizon.HTTPSERVICE;

namespace Horizon.DME
{
    public class DmeClass
    {
        private static string? CONFIG_FILE => HorizonServerConfiguration.DMEConfig;

        public static RSA_KEY? GlobalAuthPublic = null;

        public static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

        public static ServerSettings Settings = new ServerSettings();
        private static Dictionary<int, AppSettings> _appSettings = new();
        private static AppSettings _defaultAppSettings = new(0);

        public static IPAddress SERVER_IP = IPAddress.None;
        public static string? IP_TYPE;

        public static string DME_SERVER_VERSION = "3.05.0000";

        public static Dictionary<int, DMEMediusManager> Managers = new();
        public static TcpServer TcpServer = new();
        public static MediusPluginsManager Plugins = new(HorizonServerConfiguration.PluginsFolder);

        private static DateTime _timeLastPluginTick = Utils.GetHighPrecisionUtcTime();

        private static DateTime _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
        private static DateTime? _lastSuccessfulDbAuth = null;

        public static bool started = false;

        private static async Task TickAsync()
        {
            try
            {
                // Attempt to authenticate with the db middleware
                // We do this every 24 hours to get a fresh new token
                if (_lastSuccessfulDbAuth == null || (Utils.GetHighPrecisionUtcTime() - _lastSuccessfulDbAuth.Value).TotalHours > 24)
                {
                    if (!await HorizonServerConfiguration.Database.Authenticate())
                    {
                        // Log and exit when unable to authenticate
                        LoggerAccessor.LogError("Unable to authenticate with the db middleware server");

                        // disconnect from MPS
                        foreach (var manager in Managers)
                            if (manager.Value != null && manager.Value.IsConnected)
                                await manager.Value.Stop();

                        await Task.Delay(5000); // delay loop to give time before next authentication request
                        return;
                    }
                    else
                    {
                        _lastSuccessfulDbAuth = Utils.GetHighPrecisionUtcTime();

                        // refresh app settings
                        await RefreshAppSettings();

                        // reconnect to MPS
                        foreach (var manager in Managers)
                            if (manager.Value != null && !manager.Value.IsConnected)
                                await manager.Value.Start();
                    }
                }

                await HandleInMessages();

                // Tick plugins
                if ((Utils.GetHighPrecisionUtcTime() - _timeLastPluginTick).TotalMilliseconds > Settings.PluginTickIntervalMs)
                {
                    _timeLastPluginTick = Utils.GetHighPrecisionUtcTime();
                    await Plugins.Tick();
                }

                await HandleOutMessages();

                // Reload config
                if ((Utils.GetHighPrecisionUtcTime() - _lastConfigRefresh).TotalMilliseconds > Settings.RefreshConfigInterval)
                {
                    RefreshConfig();
                    _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("failed to authenticate with the mps server")) // This can happen if network unreachable, harmless as we must reconnect to MPS instead of being infinitly stuck.
                {
                    // disconnect from MPS
                    foreach (var manager in Managers)
                        if (manager.Value != null && manager.Value.IsConnected)
                            await manager.Value.Stop();

                    await Task.Delay(5000); // delay loop to give time before next authentication request
                }
                else
                    LoggerAccessor.LogError(ex);
            }
        }

        private static async Task HandleInMessages()
        {
            // handle incoming
            var InRequestsTasks = new List<Task>()
                {
                     TcpServer.HandleIncomingMessages()
                };
            foreach (var manager in Managers)
            {
                if (manager.Value.IsConnected)
                {
                    if (manager.Value.CheckMPSConnectivity())
                        InRequestsTasks.Add(manager.Value.HandleIncomingMessages());
                    else
                        await manager.Value.Stop(); // No MPS so we abort.
                }
            }

            await Task.WhenAll(InRequestsTasks);
        }

        private static async Task HandleOutMessages()
        {
            // handle outgoing
            var OutRequestsTasks = new List<Task>()
                {
                     TcpServer.HandleOutgoingMessages()
                };
            foreach (var manager in Managers)
            {
                if (manager.Value.IsConnected)
                {
                    if (manager.Value.CheckMPSConnectivity())
                        OutRequestsTasks.Add(manager.Value.HandleOutgoingMessages());
                    else
                        await manager.Value.Stop(); // No MPS so we abort.
                }
                else if ((Utils.GetHighPrecisionUtcTime() - manager.Value.TimeLostConnection)?.TotalSeconds > Settings.MPSReconnectInterval)
                    OutRequestsTasks.Add(manager.Value.Start());
            }

            await Task.WhenAll(OutRequestsTasks);
        }

        private static async Task LoopServer()
        {
            // iterate
            while (started)
            {
                // tick
                await TickAsync();

                await Task.Delay(5); // this value is the one used in Horizon by default.
            }

            await TcpServer.Stop();
            await Task.WhenAll(Managers.Select(x => x.Value.Stop()));
        }

        private static Task StartServerAsync()
        {
            try
            {
                LoggerAccessor.LogInfo("Initializing DME components...");
                LoggerAccessor.LogInfo("*****************************************************************");
                LoggerAccessor.LogInfo($"DME Message Router Version {DME_SERVER_VERSION}");

                int KM_GetSoftwareID = 120;
                LoggerAccessor.LogInfo($"DME Message Router Application ID {KM_GetSoftwareID}");

                #region DateTime
                string date = DateTime.Now.ToString("MMMM/dd/yyyy");
                string time = DateTime.Now.ToString("hh:mm:ss tt");
                LoggerAccessor.LogInfo($"Date: {date}, Time: {time}");
                #endregion

                #region DME Server Info
                LoggerAccessor.LogInfo($"Server IP = {SERVER_IP} [{IP_TYPE}]  TCP Port = {Settings.TCPPort}  UDP Port = {Settings.UDPPort}");
                TcpServer.Start();
                #endregion

                LoggerAccessor.LogInfo("*****************************************************************");
                LoggerAccessor.LogInfo($"TCP started.");

                // build and start medius managers per app id
                foreach (var applicationId in Settings.ApplicationIds)
                {
                    var manager = new DMEMediusManager(applicationId);
                    Managers.Add(applicationId, manager);
                }

                LoggerAccessor.LogInfo("DME Initalized.");

                started = true;

                _ = Task.Run(LoopServer);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[DME] - Server failed to initialize with error - {ex}");
            }

            return Task.CompletedTask;
        }

        public static void DmeMain()
        {
            RefreshConfig();
            _ = StartServerAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void RefreshConfig()
        {
            var serializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            RefreshServerIp();

            // Load settings
            if (File.Exists(CONFIG_FILE))
                // Populate existing object
                JsonConvert.PopulateObject(File.ReadAllText(CONFIG_FILE), Settings, serializerSettings);
            else
            {
                // Save defaults

                // Add the appids to the ApplicationIds list
                Settings.ApplicationIds.AddRange(new List<int>
                {
                    10683, 10684, 11354, 21914, 21624, 20764, 20371, 22500, 10540, 22920, 21731, 21834, 23624, 20043,
                    20032, 20034, 20454, 20314, 21874, 21244, 20304, 20463, 21614, 20344,
                    20434, 22204, 23360, 21513, 21064, 20804, 20374, 21094, 22274, 20060,
                    10984, 10782, 10421, 10130, 24000, 24180, 10954, 21784
                });

                Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_FILE) ?? Directory.GetCurrentDirectory() + "/static");

                File.WriteAllText(CONFIG_FILE ?? Directory.GetCurrentDirectory() + "/static/dme.json", JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }

            // Update default rsa key
            BackendProject.Horizon.LIBRARY.Pipeline.Attribute.ScertClientAttribute.DefaultRsaAuthKey = Settings.DefaultKey;

            if (Settings.DefaultKey != null)
                GlobalAuthPublic = new RSA_KEY(Settings.DefaultKey.N.ToByteArrayUnsigned().Reverse().ToArray());

            // refresh app settings
            _ = RefreshAppSettings();
        }

        private static async Task RefreshAppSettings()
        {
            try
            {
                if (!HorizonServerConfiguration.Database.AmIAuthenticated())
                    return;

                // get supported app ids
                var appIdGroups = await HorizonServerConfiguration.Database.GetAppIds();
                if (appIdGroups == null)
                    return;

                // get settings
                foreach (var appIdGroup in appIdGroups)
                {
                    foreach (var appId in appIdGroup.AppIds)
                    {
                        var settings = await HorizonServerConfiguration.Database.GetServerSettings(appId);
                        if (settings != null)
                        {
                            if (_appSettings.TryGetValue(appId, out var appSettings))
                                appSettings.SetSettings(settings);
                            else
                            {
                                appSettings = new AppSettings(appId);
                                appSettings.SetSettings(settings);
                                _appSettings.Add(appId, appSettings);

                                // we also want to send this back to the server since this is new locally
                                // and there might be new setting fields that aren't yet on the db
                                await HorizonServerConfiguration.Database.SetServerSettings(appId, appSettings.GetSettings());
                            }

                            CrudRoomManager.UpdateOrCreateRoom(Convert.ToString(appId), null, null, null, null, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError(ex);
            }
        }

        private static void RefreshServerIp()
        {
            if (!Settings.UsePublicIp)
                SERVER_IP = IPAddress.Parse(Settings.DMEIp);
            else
            {
                if (string.IsNullOrWhiteSpace(Settings.PublicIpOverride))
                    SERVER_IP = IPAddress.Parse(MiscUtils.GetPublicIPAddress());
                else
                    SERVER_IP = IPAddress.Parse(Settings.PublicIpOverride);
            }
        }

        public static ClientObject? GetClients(string appId)
        {
            return Managers.Select(x => x.Value.GetClientByAppId(appId)).FirstOrDefault(x => x != null);
        }

        public static ClientObject? GetClientByAccessToken(string accessToken)
        {
            return Managers.Select(x => x.Value.GetClientByAccessToken(accessToken)).FirstOrDefault(x => x != null);
        }

        public static ClientObject? GetClientBySessionKey(string sessionKey)
        {
            return Managers.Select(x => x.Value.GetClientBySessionKey(sessionKey)).FirstOrDefault(x => x != null);
        }

        public static AppSettings GetAppSettingsOrDefault(int appId)
        {
            if (_appSettings.TryGetValue(appId, out var appSettings))
                return appSettings;

            return _defaultAppSettings;
        }
    }
}
