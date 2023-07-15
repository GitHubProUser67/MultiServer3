using DotNetty.Common.Internal.Logging;
using Newtonsoft.Json;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Models;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Logging;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Database;
using PSMultiServer.SRC_Addons.MEDIUS.DME.Config;
using PSMultiServer.SRC_Addons.MEDIUS.DME.Models;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Plugins;
using System.Diagnostics;
using System.Net;

namespace PSMultiServer.SRC_Addons.MEDIUS.DME
{
    public class DmeClass
    {
        private static string CONFIG_DIRECTIORY = "./loginformNtemplates/DME";
        public static string CONFIG_FILE => Path.Combine(CONFIG_DIRECTIORY, "dme.json");
        public static string DB_CONFIG_FILE => Path.Combine(CONFIG_DIRECTIORY, "db.config.json");

        public static RSA_KEY GlobalAuthPublic = null;

        public static readonly Stopwatch Stopwatch = Stopwatch.StartNew();
        public static DbController Database = null;

        public static ServerSettings Settings = new ServerSettings();
        private static Dictionary<int, AppSettings> _appSettings = new Dictionary<int, AppSettings>();
        private static AppSettings _defaultAppSettings = new AppSettings(0);

        public static IPAddress SERVER_IP;
        public static string IP_TYPE;

        public static string DME_SERVER_VERSION = "3.05.0000";

        public static Dictionary<int, DMEMediusManager> Managers = new Dictionary<int, DMEMediusManager>();
        public static TcpServer TcpServer = new TcpServer();
        public static PluginsManager Plugins = null;

        private static ulong _sessionKeyCounter = 0;
        private static readonly object _sessionKeyCounterLock = _sessionKeyCounter;
        private static DateTime _timeLastPluginTick = Utils.GetHighPrecisionUtcTime();

        private static int _ticks = 0;
        private static Stopwatch _sw = new Stopwatch();
        private static HighResolutionTimer.HighResolutionTimer _timer;
        private static DateTime _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
        private static DateTime? _lastSuccessfulDbAuth = null;

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<DmeClass>();

        static int metricCooldownTicks = 0;
        static string metricPrintString = null;
        static int metricIndent = 0;

        static async Task TickAsync()
        {
            try
            {
#if DEBUG
                if (!_sw.IsRunning)
                    _sw.Start();
#endif

#if DEBUG
                ++_ticks;
                if (_sw.Elapsed.TotalSeconds > 5f)
                {
                    // 
                    _sw.Stop();
                    var averageMsPerTick = 1000 * (_sw.Elapsed.TotalSeconds / _ticks);
                    var error = Math.Abs(Settings.MainLoopSleepMs - averageMsPerTick) / Settings.MainLoopSleepMs;

                    //if (error > 0.1f)
                    //    Logger.Error($"Average Ms between ticks is: {averageMsPerTick} is {error * 100}% off of target {Settings.MainLoopSleepMs}");

                    //var dt = DateTime.UtcNow - Utils.GetHighPrecisionUtcTime();
                    //if (Math.Abs(dt.TotalMilliseconds) > 50)
                    //    Logger.Error($"System clock and local clock are out of sync! delta ms: {dt.TotalMilliseconds}");

                    _sw.Restart();
                    _ticks = 0;
                }
#endif

                // Attempt to authenticate with the db middleware
                // We do this every 24 hours to get a fresh new token
                if (_lastSuccessfulDbAuth == null || (Utils.GetHighPrecisionUtcTime() - _lastSuccessfulDbAuth.Value).TotalHours > 24)
                {
                    if (!await Database.Authenticate())
                    {
                        // Log and exit when unable to authenticate
                        Logger.Error("Unable to authenticate with the db middleware server");

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

                await TimeAsync("in", async () =>
                {
                    // handle incoming
                    {
                        var tasks = new List<Task>()
                    {
                        TcpServer.HandleIncomingMessages()
                    };

                        foreach (var manager in Managers)
                        {
                            if (manager.Value.IsConnected)
                            {
                                tasks.Add(manager.Value.HandleIncomingMessages());
                            }
                        }

                        await Task.WhenAll(tasks);
                    }
                });

                await TimeAsync("plugins", async () =>
                {
                    // Tick plugins
                    if ((Utils.GetHighPrecisionUtcTime() - _timeLastPluginTick).TotalMilliseconds > Settings.PluginTickIntervalMs)
                    {
                        _timeLastPluginTick = Utils.GetHighPrecisionUtcTime();
                        await Plugins.Tick();
                    }
                });

                await TimeAsync("out", async () =>
                {
                    // handle outgoing
                    {
                        var tasks = new List<Task>()
                        {
                            TcpServer.HandleOutgoingMessages()
                        };
                        foreach (var manager in Managers)
                        {
                            if (manager.Value.IsConnected)
                            {
                                tasks.Add(manager.Value.HandleOutgoingMessages());
                            }
                            else if ((Utils.GetHighPrecisionUtcTime() - manager.Value.TimeLostConnection)?.TotalSeconds > Settings.MPSReconnectInterval)
                            {
                                tasks.Add(manager.Value.Start());
                            }
                        }

                        await Task.WhenAll(tasks);
                    }
                });

                // Reload config
                if ((Utils.GetHighPrecisionUtcTime() - _lastConfigRefresh).TotalMilliseconds > Settings.RefreshConfigInterval)
                {
                    RefreshConfig();
                    _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                await TcpServer.Stop();
                await Task.WhenAll(Managers.Select(x => x.Value.Stop()));
            }
        }

        static async Task StartServerAsync()
        {
            int waitMs = Settings.MainLoopSleepMs;

            Logger.Info("Initializing DME components...");
            Logger.Info("*****************************************************************");
            Logger.Info($"DME Message Router Version {DME_SERVER_VERSION}");

            int KM_GetSoftwareID = 120;
            Logger.Info($"DME Message Router Application ID {KM_GetSoftwareID}");

            #region DateTime
            string date = DateTime.Now.ToString("MMMM/dd/yyyy");
            string time = DateTime.Now.ToString("hh:mm:ss tt");
            Logger.Info($"Date: {date}, Time: {time}");
            #endregion

            #region DME Server Info
            Logger.Info($"Server IP = {SERVER_IP} [{IP_TYPE}]  TCP Port = {Settings.TCPPort}  UDP Port = {Settings.UDPPort}");
            TcpServer.Start();
            #endregion

            #region ConfigAuxUDP Check

            if (Settings.EnableAuxUDP)
            {
                Logger.Info("Auxilary UDP is ENABLED!\n");
            }
            else
            {
                Logger.Info("Auxilary UDP is DISABLED!\n");
            }

            #endregion

            Logger.Info("*****************************************************************");
            Logger.Info($"TCP started.");

            // build and start medius managers per app id
            foreach (var applicationId in Settings.ApplicationIds)
            {
                var manager = new DMEMediusManager(applicationId);
                //ogger.Info($"Starting MPS for appid {applicationId}.");
                //await manager.Start();
                //Logger.Info($"MPS started.");
                Managers.Add(applicationId, manager);
            }

            // 
            Logger.Info("DME Initalized.");

            // start timer
            _timer = new HighResolutionTimer.HighResolutionTimer();
            _timer.SetPeriod(waitMs);
            _timer.Start();

            // iterate
            while (true)
            {
                // 
                if (metricCooldownTicks > 0)
                    metricCooldownTicks--;
                else
                    metricCooldownTicks = 1000 * 5 / waitMs; // 5 seconds

                // handle tick rate change
                if (Settings.MainLoopSleepMs != waitMs)
                {
                    waitMs = Settings.MainLoopSleepMs;
                    _timer.Stop();
                    _timer.SetPeriod(waitMs);
                    _timer.Start();
                }

                // tick
                await TimeAsync("tick", TickAsync);

                if (GetLogs.Logging.LogMetrics && !String.IsNullOrEmpty(metricPrintString))
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                        Logger.Info("\n" + metricPrintString);
                    else
                        Logger.Info(metricPrintString);
                    metricPrintString = "";
                }

                var l1 = Stopwatch.ElapsedMilliseconds;

                // wait for next tick
                _timer.WaitForTrigger();
                //Thread.Sleep(TimeSpan.FromTicks((long)(TimeSpan.TicksPerMillisecond * 0.9)));

                var l2 = Stopwatch.ElapsedMilliseconds;

                if (l2 - l1 > 1)
                {
                    //Logger.Error($"LOOP DT {l2 - l1}");
                }
            }
        }

        public static async Task DmeMain()
        {
            // 
            Database = new DbController(DB_CONFIG_FILE);

            // 
            Initialize();

            // Initialize plugins
            Plugins = new PluginsManager(Settings.PluginsPath);

            // 
            await StartServerAsync();
        }

        static void Initialize()
        {
            RefreshServerIp();
            RefreshConfig();
        }

        /// <summary>
        /// 
        /// </summary>
        static void RefreshConfig()
        {
            var usePublicIp = Settings.UsePublicIp;

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

                // Add the appids to the ApplicationIds list
                Settings.ApplicationIds.AddRange(new List<int>
                {
                    21624, 20764, 20371, 22500, 10540, 22920, 21731, 21834, 23624, 20043,
                    20032, 20034, 20454, 20314, 21874, 21244, 20304, 20463, 21614, 20344,
                    20434, 22204, 23360, 21513, 21064, 20804, 20374, 21094, 22274, 20060,
                    10984, 10782, 10421, 10130, 24000, 24180
                });

                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }

            // Update default rsa key
            Server.Pipeline.Attribute.ScertClientAttribute.DefaultRsaAuthKey = Settings.DefaultKey;

            if (Settings.DefaultKey != null)
                GlobalAuthPublic = new RSA_KEY(Settings.DefaultKey.N.ToByteArrayUnsigned().Reverse().ToArray());

            // Determine server ip
            if (usePublicIp != Settings.UsePublicIp)
                RefreshServerIp();


            // refresh app settings
            _ = RefreshAppSettings();
        }

        static async Task RefreshAppSettings()
        {
            try
            {
                if (!await Database.AmIAuthenticated())
                    return;

                // get supported app ids
                var appIdGroups = await Database.GetAppIds();
                if (appIdGroups == null)
                    return;

                // get settings
                foreach (var appIdGroup in appIdGroups)
                {
                    foreach (var appId in appIdGroup.AppIds)
                    {
                        var settings = await Database.GetServerSettings(appId);
                        if (settings != null)
                        {
                            if (_appSettings.TryGetValue(appId, out var appSettings))
                            {
                                appSettings.SetSettings(settings);
                            }
                            else
                            {
                                appSettings = new AppSettings(appId);
                                appSettings.SetSettings(settings);
                                _appSettings.Add(appId, appSettings);

                                // we also want to send this back to the server since this is new locally
                                // and there might be new setting fields that aren't yet on the db
                                await Database.SetServerSettings(appId, appSettings.GetSettings());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        static void RefreshServerIp()
        {
            if (!Settings.UsePublicIp)
            {
                SERVER_IP = Utils.GetLocalIPAddress();
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Settings.PublicIpOverride))
                    SERVER_IP = IPAddress.Parse(Utils.GetPublicIPAddress());
                else
                    SERVER_IP = IPAddress.Parse(Settings.PublicIpOverride);
            }
        }

        public static DMEMediusManager GetManager(int applicationId, bool useDefaultOnMissing)
        {
            if (Managers.TryGetValue(applicationId, out var manager))
                return manager;

            if (useDefaultOnMissing && Managers.TryGetValue(0, out manager))
                return manager;

            return null;
        }

        public static ClientObject GetClientByAccessToken(string accessToken)
        {
            return Managers.Select(x => x.Value.GetClientByAccessToken(accessToken)).FirstOrDefault(x => x != null);
        }

        public static string GenerateSessionKey()
        {
            lock (_sessionKeyCounterLock)
            {
                return (++_sessionKeyCounter).ToString();
            }
        }

        public static AppSettings GetAppSettingsOrDefault(int appId)
        {
            if (_appSettings.TryGetValue(appId, out var appSettings))
                return appSettings;

            return _defaultAppSettings;
        }

        #region Metrics

        public static void Time(string name, Action action)
        {
            if (metricCooldownTicks > 0)
            {
                action();
                return;
            }

            // 
            long ticksAtStart = Stopwatch.ElapsedTicks;

            // insert row before action
            metricPrintString += $"({"".PadRight(metricIndent * 2, ' ') + name,-32}:    {100:#.000} ms)\n";
            int stringIndex = metricPrintString.Length - 5 - 7;

            // run
            ++metricIndent;
            try
            {
                action();
            }
            finally
            {
                --metricIndent;
            }

            //
            long ticksAfterAction = Stopwatch.ElapsedTicks;
            var actionDurationMs = 1000f * (ticksAfterAction - ticksAtStart) / Stopwatch.Frequency;

            //
            var replacementString = actionDurationMs.ToString("#.000").PadLeft(7, ' ').Substring(0, 7);
            char[] charArr = metricPrintString.ToCharArray();
            replacementString.CopyTo(0, charArr, stringIndex, replacementString.Length);
            metricPrintString = new string(charArr);
        }

        public static async Task TimeAsync(string name, Func<Task> action)
        {
            if (metricCooldownTicks > 0)
            {
                await action();
                return;
            }

            // 
            long ticksAtStart = Stopwatch.ElapsedTicks;

            // insert row before action
            metricPrintString += $"({"".PadRight(metricIndent * 2, ' ') + name,-32}:    {100:#.000} ms)\n";
            int stringIndex = metricPrintString.Length - 5 - 7;

            // run
            ++metricIndent;
            try
            {
                await action();
            }
            finally
            {
                --metricIndent;
            }

            //
            long ticksAfterAction = Stopwatch.ElapsedTicks;
            var actionDurationMs = 1000f * (ticksAfterAction - ticksAtStart) / Stopwatch.Frequency;

            //
            var replacementString = actionDurationMs.ToString("#.000").PadLeft(7, ' ').Substring(0, 7);
            char[] charArr = metricPrintString.ToCharArray();
            replacementString.CopyTo(0, charArr, stringIndex, replacementString.Length);
            metricPrintString = new string(charArr);
        }

        public static async Task<T> TimeAsync<T>(string name, Func<Task<T>> action)
        {
            T result;
            if (metricCooldownTicks > 0)
            {
                return await action();
            }

            // 
            long ticksAtStart = Stopwatch.ElapsedTicks;

            // insert row before action
            metricPrintString += $"({"".PadRight(metricIndent * 2, ' ') + name,-32}:    {100:#.000} ms)\n";
            int stringIndex = metricPrintString.Length - 5 - 7;

            // run
            ++metricIndent;
            try
            {
                result = await action();
            }
            finally
            {
                --metricIndent;
            }

            //
            long ticksAfterAction = Stopwatch.ElapsedTicks;
            var actionDurationMs = 1000f * (ticksAfterAction - ticksAtStart) / Stopwatch.Frequency;

            //
            var replacementString = actionDurationMs.ToString("#.000").PadLeft(7, ' ').Substring(0, 7);
            char[] charArr = metricPrintString.ToCharArray();
            replacementString.CopyTo(0, charArr, stringIndex, replacementString.Length);
            metricPrintString = new string(charArr);

            return result;
        }

        #endregion

    }
}
