using DotNetty.Common.Internal.Logging;
using Newtonsoft.Json;
using PSMultiServer.SRC_Addons.MEDIUS.SVO.Config;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common;
using System.Diagnostics;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Database;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Plugins;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common.Logging;

namespace PSMultiServer.SRC_Addons.MEDIUS.SVO
{
    public class SvoClass
    {
        private static string CONFIG_DIRECTIORY = "./loginformNtemplates/SVO";
        public static string CONFIG_FILE => Path.Combine(CONFIG_DIRECTIORY, "svo.json");
        public static string DB_CONFIG_FILE => Path.Combine(CONFIG_DIRECTIORY, "db.config.json");

        public static string HOMEmessageoftheday = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
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

        public static ServerSettings Settings = new ServerSettings();
        public static SVO SVOServer = new SVO();
        public static DbController Database = null;


        public static SVOManager Manager = new SVOManager();
        public static PluginsManager Plugins = null;

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<SvoClass>();


        //public static Init libAntiCheat = new Init();

        private static Dictionary<int, AppSettings> _appSettings = new Dictionary<int, AppSettings>();
        private static AppSettings _defaultAppSettings = new AppSettings(0);


        public static int TickMS => 1000 / (Settings?.TickRate ?? 10);

        private static ulong _sessionKeyCounter = 0;
        private static int sleepMS = 0;
        private static readonly object _sessionKeyCounterLock = _sessionKeyCounter;
        private static DateTime? _lastSuccessfulDbAuth = null;
        private static DateTime _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
        private static DateTime _lastComponentLog = Utils.GetHighPrecisionUtcTime();


        private static int _ticks = 0;
        private static Stopwatch _sw = new Stopwatch();
        private static HighResolutionTimer.HighResolutionTimer _timer;

        static async Task TickAsync()
        {
            try
            {
#if DEBUG || RELEASE
                if (!_sw.IsRunning)
                    _sw.Start();
#endif

#if DEBUG || RELEASE
                ++_ticks;
                if (_sw.Elapsed.TotalSeconds > 5f)
                {
                    // 
                    _sw.Stop();
                    float tps = _ticks / (float)_sw.Elapsed.TotalSeconds;
                    float error = MathF.Abs(Settings.TickRate - tps) / Settings.TickRate;

                    if (error > 0.1f)
                        Logger.Error($"Average TickRate Per Second: {tps} is {error * 100}% off of target {Settings.TickRate}");
                    //var dt = DateTime.UtcNow - Utils.GetHighPrecisionUtcTime();
                    //if (Math.Abs(dt.TotalMilliseconds) > 50)
                    //    Logger.Error($"System clock and local clock are out of sync! delta ms: {dt.TotalMilliseconds}");

                    _sw.Restart();
                    _ticks = 0;
                }
#endif

                // Attempt to authenticate with the db middleware
                // We do this every 24 hours to get a fresh new token
                if ((_lastSuccessfulDbAuth == null || (Utils.GetHighPrecisionUtcTime() - _lastSuccessfulDbAuth.Value).TotalHours > 24))
                {
                    if (!await Database.Authenticate())
                    {
                        // Log and exit when unable to authenticate
                        Logger.Error($"Unable to authenticate connection to Cache Server.");
                        return;
                    }
                    else
                    {
                        _lastSuccessfulDbAuth = Utils.GetHighPrecisionUtcTime();

                        // pass to manager
                        await Manager.OnDatabaseAuthenticated();

                        // refresh app settings
                        await RefreshAppSettings();

                        #region Check Cache Server Simulated
                        if (Database._settings.SimulatedMode != true)
                        {
                            Logger.Info("Connected to Cache Server");
                        }
                        else
                        {
                            Logger.Info("Connected to Cache Server (Simulated)");
                        }
                        #endregion
                        /*
                        #if !DEBUG
                                                if (!_hasPurgedAccountStatuses)
                                                {
                                                    _hasPurgedAccountStatuses = await Database.ClearAccountStatuses();
                                                    await Database.ClearActiveGames();
                                                }
                        #endif
                        */
                    }
                }

                // Tick Profiling

                // prof:* Total Number of Connect Attempts (%d), Number Disconnects (%d), Total On (%d)
                // 
                //Logger.Info($"prof:* Total Server Uptime = {GetUptime()} Seconds == (%d days, %d hours, %d minutes, %d seconds)");

                //Logger.Info($"prof:* Total Available RAM = {} bytes");

                // Tick
                await Task.WhenAll(
                    SVOServer.Tick());

                // Tick manager
                await Manager.Tick();

                // Tick plugins
                await Plugins.Tick();

                // 
                if ((Utils.GetHighPrecisionUtcTime() - _lastComponentLog).TotalSeconds > 15f)
                {
                    _lastComponentLog = Utils.GetHighPrecisionUtcTime();
                }

                // Reload config
                if ((Utils.GetHighPrecisionUtcTime() - _lastConfigRefresh).TotalMilliseconds > Settings.RefreshConfigInterval)
                {
                    await RefreshConfig();
                    _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                //STOP SVO
            }
        }

        static async Task StartServerAsync()
        {

            int waitMs = sleepMS;

            Logger.Info("**************************************************");
            string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");
            Logger.Info($"* Launched on {datetime}");

            Task.WaitAll(SVOServer.Start());
            Logger.Info($"SVO started. . .");

            #region Timer
            // start timer
            _timer = new HighResolutionTimer.HighResolutionTimer();
            _timer.SetPeriod(waitMs);
            _timer.Start();

            // iterate
            while (true)
            {
                // handle tick rate change
                if (sleepMS != waitMs)
                {
                    waitMs = sleepMS;
                    _timer.Stop();
                    _timer.SetPeriod(waitMs);
                    _timer.Start();
                }

                // tick
                await TickAsync();

                // wait for next tick
                _timer.WaitForTrigger();
            }

            #endregion
            /*
            try
            {
                while (true)
                {
                    await SVOServer.Tick();

                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {

            }
            */
        }

        public static async Task SvoMain(string _HOMEmessageoftheday)
        {
            HOMEmessageoftheday = _HOMEmessageoftheday;

            // 
            Database = new DbController(DB_CONFIG_FILE);

            // Initialize plugins
            Plugins = new PluginsManager(Settings.PluginsPath);

            await Initialize();

            // 
            await StartServerAsync();
        }

        static async Task Initialize()
        {
            await RefreshConfig();

            // 
            var serializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
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

                //Prefetch

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static Task RefreshConfig()
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
                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(Settings, Newtonsoft.Json.Formatting.Indented));
            }

            // Set LogSettings singleton
            LogSettings.Singleton = GetLogs.Logging;

            // Update default rsa key
            //Pipeline.Attribute.ScertClientAttribute.DefaultRsaAuthKey = Settings.DefaultKey;

            // Update file logger min level
            if (GetLogs._fileLogger != null)
                GetLogs._fileLogger.MinLevel = GetLogs.Logging.LogLevel;

            //
            _ = RefreshAppSettings();

            // Load tick time into sleep ms for main loop
            sleepMS = TickMS;
            return Task.CompletedTask;
        }
    }
}
