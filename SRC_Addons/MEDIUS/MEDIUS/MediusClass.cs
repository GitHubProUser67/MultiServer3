using DotNetty.Common.Internal.Logging;
using Newtonsoft.Json;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Common;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Models;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Common;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Database;
using PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.Config;
using PSMultiServer.SRC_Addons.MEDIUS.MEDIUS.Medius.Models;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Plugins;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using PSMultiServer.SRC_Addons.MEDIUS.Server.libAntiCheat;

namespace PSMultiServer.SRC_Addons.MEDIUS.MEDIUS
{
    public class MediusClass
    {
        private static string CONFIG_DIRECTIORY = "./loginformNtemplates/MEDIUS";
        public static string CONFIG_FILE => Path.Combine(CONFIG_DIRECTIORY, "medius.json");
        public static string DB_CONFIG_FILE => Path.Combine(CONFIG_DIRECTIORY, "db.config.json");

        public static RSA_KEY GlobalAuthPublic = null;

        public static ServerSettings Settings = new ServerSettings();
        public static DbController Database = null;

        //public static Init libAntiCheat = new Init();

        public static IPAddress SERVER_IP;
        public static string IP_TYPE;

        public static Medius.MediusManager Manager = new Medius.MediusManager();
        public static PluginsManager Plugins = null;

        public SECURITY_MODE eSecurityMode = SECURITY_MODE.MODE_UNKNOWN;

        public static Medius.MAPS ProfileServer = new Medius.MAPS();
        public static Medius.MMS MatchmakingServer = new Medius.MMS();
        public static Medius.MAS AuthenticationServer = new Medius.MAS();
        public static Medius.MLS LobbyServer = new Medius.MLS();
        public static Medius.MPS ProxyServer = new Medius.MPS();

        public static AntiCheat AntiCheatPlugin = new AntiCheat();
        public static Server.libAntiCheat.Models.ClientObject AntiCheatClient = new Server.libAntiCheat.Models.ClientObject();

        public static int TickMS => 1000 / (Settings?.TickRate ?? 10);

        private static Dictionary<int, AppSettings> _appSettings = new Dictionary<int, AppSettings>();
        private static AppSettings _defaultAppSettings = new AppSettings(0);
        private static ulong _sessionKeyCounter = 0;
        private static int sleepMS = 0;
        private static readonly object _sessionKeyCounterLock = _sessionKeyCounter;
        private static DateTime? _lastSuccessfulDbAuth = null;
        private static DateTime _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();
        private static DateTime _lastComponentLog = Utils.GetHighPrecisionUtcTime();

        private static int _ticks = 0;
        private static Stopwatch _sw = new Stopwatch();
        private static HighResolutionTimer.HighResolutionTimer _timer;

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<MediusClass>();

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
                        Logger.Info("Successfully authenticated with the db middleware server");

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
                    AuthenticationServer.Tick(),
                    LobbyServer.Tick(),
                    ProxyServer.Tick(),
                    ProfileServer.Tick(),
                    MatchmakingServer.Tick());

                // Tick manager
                await Manager.Tick();

                // Tick plugins
                await Plugins.Tick();

                // 
                if ((Utils.GetHighPrecisionUtcTime() - _lastComponentLog).TotalSeconds > 15f)
                {
                    AuthenticationServer.Log();
                    LobbyServer.Log();
                    ProxyServer.Log();
                    ProfileServer.Log();
                    MatchmakingServer.Log();
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

                await AuthenticationServer.Stop();
                await LobbyServer.Stop();
                await ProxyServer.Stop();
                await ProfileServer.Stop();
                await MatchmakingServer.Stop();
            }
        }

        static async Task StartServerAsync()
        {
            int waitMs = sleepMS;
            string AppIdArray = null; //string.Join(", ", Settings.ApplicationIds);

            Logger.Info("Initializing medius components...");
            //Program
            Logger.Info("**************************************************");

            #region MediusGetBuildTimeStamp
            var MediusBuildTimeStamp = GetLinkerTime(Assembly.GetEntryAssembly());
            Logger.Info($"* MediusBuildTimeStamp at {MediusBuildTimeStamp}");
            #endregion

            string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");
            Logger.Info($"* Launched on {datetime}");

            // Get the current process.
            Process currentProcess = Process.GetCurrentProcess();

            //Parent ProcessId
            Logger.Info($"* Process ID: {currentProcess.Id}");

            if (Database._settings.SimulatedMode == true)
            {
                Logger.Info("* Database Disabled Medius Stack");
            }
            else
            {
                Logger.Info("* Database Enabled Medius Stack");
            }

            #region KeyMaster
            //Use PublicKeyType
            //Logger.Info($"* Server Key Type: {Settings.EncryptMessages}");

            #endregion

            #region Remote Log Viewing
            if (Settings.RemoteLogViewPort == 0)
            {
                //* Remote log viewing setup failure with port %d.
                Logger.Info("* Remote log viewing disabled.");
            }
            else if (Settings.RemoteLogViewPort > 0)
            {
                Logger.Info($"* Remote log viewing enabled at port {Settings.RemoteLogViewPort}.");
            }
            #endregion

            Logger.Info("**************************************************");

            #region Anti-Cheat Init (WIP)
            if (Settings.AntiCheatOn == true)
            {
                await AntiCheatPlugin.AntiCheatInit(LM_SEVERITY_LEVEL.LM_INFO, Settings.AntiCheatOn);
            }
            #endregion

            #region MediusGetVersion
            if (Settings.MediusServerVersionOverride == true)
            {
                #region MAPS - Zipper Interactive MAG/Socom 4
                if (Settings.EnableMAPS == true)
                {
                    Logger.Info($"MAPS Version: {Settings.MAPSVersion}");
                    Logger.Info($"Enabling MAPS on Server IP = {SERVER_IP} TCP Port = {AuthenticationServer.TCPPort} UDP Port = {AuthenticationServer.UDPPort}.");

                    ProfileServer.Start();
                    Logger.Info("Medius Profile Server Intialized and Now Accepting Clients");
                }
                #endregion

                #region MMS Enabled?
                if (Settings.EnableMMS == true)
                {
                    #region MAS 
                    Logger.Info($"MMS Version: {Settings.MMSVersion}");
                    Logger.Info($"Enabling MMS on Server IP = {SERVER_IP} TCP Port = {MatchmakingServer.TCPPort} UDP Port = {MatchmakingServer.UDPPort}.");
                    //Logger.Info($"Medius Matchmaking Server running under ApplicationID {AppIdArray}");

                    //Connecting to Medius Universe Manager 127.0.0.1 10076 1
                    //Connected to Universe Manager server

                    MatchmakingServer.Start();
                    Logger.Info("Medius Matchmaking Server Initialized");
                    #endregion

                }
                #endregion

                #region MAS Enabled?
                if (Settings.EnableMAS == true)
                {
                    #region MAS 
                    Logger.Info($"MAS Version: {Settings.MASVersion}");
                    Logger.Info($"Enabling MAS on Server IP = {SERVER_IP} TCP Port = {AuthenticationServer.TCPPort} UDP Port = {AuthenticationServer.UDPPort}.");
                    //Logger.Info($"Medius Authentication Server running under ApplicationID {AppIdArray}");

                    //Connecting to Medius Universe Manager 127.0.0.1 10076 1
                    //Connected to Universe Manager server

                    AuthenticationServer.Start();
                    Logger.Info("Medius Authentication Server Initialized");
                    #endregion

                }
                #endregion

                #region MLS Enabled?
                if (Settings.EnableMLS == true)
                {
                    Logger.Info($"MLS Version: {Settings.MLSVersion}");
                    Logger.Info($"Enabling MLS on Server IP = {SERVER_IP} TCP Port = {LobbyServer.TCPPort} UDP Port = {LobbyServer.UDPPort}.");
                    Logger.Info($"Medius Lobby Server running under ApplicationID {AppIdArray}");

                    DMEServerResetMetrics();

                    LobbyServer.Start();
                    Logger.Info("Medius Lobby Server Initialized and Now Accepting Clients");
                }
                #endregion

                #region MPS Enabled?
                if (Settings.EnableMPS == true)
                {
                    Logger.Info($"MPS Version: {Settings.MPSVersion}");
                    Logger.Info($"Enabling MPS on Server IP = {SERVER_IP} TCP Port = {ProxyServer.TCPPort}.");
                    //Logger.Info($"Medius Proxy Server running under ApplicationID {AppIdArray}");

                    ProxyServer.Start();
                    Logger.Info("Medius Proxy Server Initialized and Now Accepting Clients");

                    //Connecting to Medius Universe Manager 127.0.0.1 10076 1
                    //Connected to Universe Manager server after 1 attempts
                }
                #endregion
            }
            else
            {

                #region MAPS - Zipper Interactive MAG/Socom 4
                if (Settings.EnableMAPS == true)
                {
                    Logger.Info($"Enabling MAPS on Server IP = {SERVER_IP} TCP Port = {ProfileServer.TCPPort} UDP Port = {ProfileServer.UDPPort}.");
                    //Logger.Info($"Medius Profile Server running under ApplicationID {AppIdArray}");

                    ProfileServer.Start();
                    Logger.Info("Medius Profile Server Intialized and Now Accepting Clients");
                }
                #endregion

                #region MMS Enabled?
                if (Settings.EnableMMS == true)
                {
                    #region MAS 
                    Logger.Info($"MMS Version: {Settings.MMSVersion}");
                    Logger.Info($"Enabling MMS on Server IP = {SERVER_IP} TCP Port = {MatchmakingServer.TCPPort} UDP Port = {MatchmakingServer.UDPPort}.");
                    //Logger.Info($"Medius Matchmaking Server running under ApplicationID {AppIdArray}");

                    //Connecting to Medius Universe Manager 127.0.0.1 10076 1
                    //Connected to Universe Manager server

                    MatchmakingServer.Start();
                    Logger.Info("Medius Matchmaking Server Initialized");
                    #endregion

                }
                #endregion

                #region MAS
                if (Settings.EnableMAS == true)
                {
                    Logger.Info($"MAS Version: {Settings.MASVersion}");
                    Logger.Info($"Enabling MAS on Server IP = {SERVER_IP} TCP Port = {AuthenticationServer.TCPPort} UDP Port = {AuthenticationServer.UDPPort}.");
                    //Logger.Info($"Medius Authentication Server running under ApplicationID {AppIdArray}");

                    //Connecting to Medius Universe Manager 127.0.0.1 10076 1
                    //Connected to Universe Manager server

                    AuthenticationServer.Start();
                    Logger.Info("Medius Authentication Server Initialized");

                }
                #endregion

                #region MLS Enabled?
                if (Settings.EnableMAS == true)
                {
                    Logger.Info($"MLS Version: {Settings.MLSVersion}");
                    Logger.Info($"Enabling MLS on Server IP = {SERVER_IP} TCP Port = {LobbyServer.TCPPort} UDP Port = {LobbyServer.UDPPort}.");
                    //Logger.Info($"Medius Lobby Server running under ApplicationID {AppIdArray}");

                    //DMEServerResetMetrics();

                    LobbyServer.Start();
                    Logger.Info("Medius Lobby Server Initialized and Now Accepting Clients");
                }
                #endregion

                #region MPS Enabled?
                if (Settings.EnableMPS == true)
                {
                    Logger.Info($"MPS Version: {Settings.MPSVersion}");
                    Logger.Info($"Enabling MPS on Server IP = {SERVER_IP} TCP Port = {ProxyServer.TCPPort}.");
                    //Logger.Info($"Medius Proxy Server running under ApplicationID {AppIdArray}");

                    ProxyServer.Start();
                    Logger.Info("Medius Proxy Server Initialized and Now Accepting Clients");

                    //Connecting to Medius Universe Manager 127.0.0.1 10076 1
                    //Connected to Universe Manager server after 1 attempts
                }
                #endregion

                // Use hardcoded methods in code to handle specific games server versions
                Logger.Info("Using Game Specific Server Versions");
            }


            #region NAT
            //Get NATIp
            if (Settings.NATIp != null)
            {
                try
                {
                    IPHostEntry host = Dns.GetHostEntry(Settings.NATIp);

                    if (Settings.NATIp != host.HostName)
                    {
                        IPAddress ip = IPAddress.Parse(host.AddressList.First().ToString());
                        ip.MapToIPv4();
                        try
                        {
                            DoGetHostAddressEntry(ip);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error($"Unable to resolve NAT service IP: {ip}  Exiting with exception: {ex}");
                            Environment.Exit(1);
                        }
                    }
                    else
                    {
                        DoGetHostNameEntry(Settings.NATIp);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Unable to resolve NAT service IP: {Settings.NATIp}  Exiting with exception: {ex}");
                    Environment.Exit(1);
                }


            }
            #endregion


            //* Diagnostic Profiling Enabled: %d Counts

            //Test:NGS Environment flag: %d

            //Billing Service Provider

            //Server-Side Vulgarity Filter Switch 
            //Valid Characters= %s
            //Dictionary Hard[%s] SoftNo[%s] SoftYes[%s] Substring[%s] Substring[%s]

            //ERROR: Could not reset DME Svr metrics[%d]?
            //TOMUM -  SEND PERCENTAGE[%d] RECV PERCENTAGE [%d]
            //DME SVR -  SEND BYTES[%ld] RECV BYTES[%ld]
            //SYS -  MAX SYS[%f]
            //Error initializing MediusTimer.  Continuing...

            //MediusParseLadderList0AppIDs
            //BinaryParseInitialize

            /*
            string rt_msg_client_get_version_string = "rt_msg_client version: 1.08.0206";
            Logger.Info($"Initialized Message Client Library {rt_msg_client_get_version_string}");
            */

            //DMEServer Enabling Dynamic Client Memory
            //rt_msg_server_enable_dynamic_client_memory
            //DmeServerEnableDynamicClientMemory failed. Continuing

            //Messaging Version = %d.%02d.%04d
            //%s library version = %d.%02d.%04d
            //Medius Lobby Server Intialized and Now Accepting Clients

            //Unable to connect to Cache Server. Error %d
            //Connected to Cache Server

            //MediusConnectLobbyToMUCG
            //Connecting to MUCG %s %d %d
            //MUCGIP
            //MUCGPort
            //WorldID
            //MUMIP
            //MUMPort
            //ForwardChatMsgFromMUCG
            //ForwardClanChatMsgFromMUCG
            //CurrentOnlineCount return value
            // Recovery Callback
            //EventsRequested
            //MUCGProcessSyncCB
            //MediusMUCGEventCB

            //Error connecting to MUCG

            //MUCGSendSync returned error %d

            //MFS_ProcessDownloadRequests
            //Error processing download queue. Error %d
            //MFS_ProcessUploadRequests
            //Error processing upload queue. Error %d

            //socket:Lost connection to Cache Server. Reconnecting[%d]
            //Unable to connect to Cache Server. Error %d
            //"Connected to Cache Server

            //ERR: LOST CONNECTION WITH MUM -- Cleaning up DME Worlds.  ATTEMPTING TO RE-ESTABLISH!!

            //MediusConnectLobbyToMUM

            //MUCGGetConnectState
            //Lost Connection to MUCG. Will attempt reconnect in %d seconds

            //ForceConfigReload
            //
            //ConfigManager Cannot Reload Configuration File %d

            //Reloading Dictionary Files
            //clSoftNo
            //clSoftYes
            //clHard
            //FPATExists
            //load_fpat
            //read_file_type
            //read_fpat
            //Incorrect file type in %s.\n
            //Unable to open %s.\n
            //fpat
            //loadclassifier

            //DmeServerUpdateAllWorlds
            //update:Error Code %d Updating All Worlds
            //update:DME Server Network Error = %d

            //AOSCacheFlushCheck
            //Error during AOS cache flush

            //BillingProviderProcessResultQueue
            //BillingProviderProcessResultQueue error.

            //RunningStatus
            //Shutting down NotificationScheduler.
            //Error shutting down MFS download queue
            //Error shutting down MFS upload queue

            //DMEServerCleanup or DMEServerCleanupWorld
            //update:DmeServerCleanupWorld error - world %d error =%d
            //update:DmeServerCleanup error =%d

            //Destroy Billing
            //BillingProviderDestroy
            //update:Ending Medius Lobby Server Operations

            //CacheDestroy
            //clSoftNo
            //clSoftYes
            //clHard
            //fpat

            //pendingTransClose
            //destroyMediusHttpd
            //MFS_transferDestroy
            //ClanCache_Destroy
            //Deleting ClanCache Error: %d

            #endregion

            Logger.Info("Medius Stacks Initialized");
            /*
            #region MFS
            if (!GetAppSettingsOrDefault(appId).EnableMediusFileServices == true)
            {
                Logger.Info($"Initializing MFS Download Queue of size {Settings.MFSDownloadQSize}");

                Logger.Info($"Initializing MFS Upload Queue of size {Settings.MFSUploadQSize}");
                MFS_transferInit();

                Logger.Info($"MFS Queue Timeout Interval {Settings.MFSQueueTimeoutInterval}");
            }
            #endregion
            */

            #region Timer
            // start timer
            _timer = new SRC_Addons.HighResolutionTimer.HighResolutionTimer();
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
        }


        public static async Task MediusMain()
        {
            // 
            Database = new DbController(DB_CONFIG_FILE);

            // 
            await Initialize();

            /*
            string root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string subdirLogs = root + @"\logs";
            string subdirMFSFiles = root + @"\MFSFiles";
            string subdirConfig = root + @"\config";
            string subdirHorizonPlugins = root + @"\plugins";


            #region Create Dirs
            // If Logs directory does not exist, create it. 
            if (!Directory.Exists(subdirLogs))
            {
                Directory.CreateDirectory(subdirLogs);
            }

            // If MFSFiles directory does not exist, create it. 
            if (!Directory.Exists(subdirMFSFiles))
            {
                Directory.CreateDirectory(subdirMFSFiles);
            }

            // If config directory does not exist, create it. 
            if (!Directory.Exists(subdirConfig))
            {
                Directory.CreateDirectory(subdirConfig);
            }
            #endregion
            */

            // Initialize plugins
            Plugins = new PluginsManager(Settings.PluginsPath);

            // 
            await StartServerAsync();
        }

        static async Task Initialize()
        {
            await RefreshConfig().ContinueWith(r =>
            {
                if (r.IsCompletedSuccessfully && r.IsCompleted)
                {
                    Logger.Info("Reloading Config File");
                }
                else
                {
                    Logger.Info($"ConfigManager Cannot Reload Configuration File {CONFIG_FILE}");
                }
                //Program.AntiCheatPlugin.mc_anticheat_event(AnticheatEventCode.anticheatLEAVEGAME, data.ClientObject.WorldId, data.ClientObject.AccountId, Program.AntiCheatClient, updateUserState, 256);
            });

            /*
            #region Locations TEMP
            if (Settings.Locations != null)
            {
                foreach (var location in Settings.Locations)
                {
                    if (location.AppIds == null || location.AppIds.Length == 0)
                    {
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = 0,
                            MaxPlayers = 256,
                            Name = location.ChannelName ?? location.Name,
                            GenericFieldLevel = location.GenericFieldLevel,
                            Id = location.Id,
                            Type = ChannelType.Lobby
                        });
                    }
                    else
                    {
                        foreach (var appId in location.AppIds)
                        {
                            Manager.AddChannel(new Channel()
                            {
                                ApplicationId = appId,
                                MaxPlayers = 256,
                                Name = location.ChannelName ?? location.Name,
                                GenericFieldLevel = location.GenericFieldLevel,
                                Id = location.Id,
                                Type = ChannelType.Lobby
                            });
                        }
                    }
                }
            }
            #endregion
            /*
            if (Settings.ApplicationIds != null)
            {
                foreach (var appId in Settings.ApplicationIds)
                {

                    #region Motorstorm 1 / Monument Valley
                    //SCEA SCEE SCEI SCEK
                    else if (appId == 20764 || appId == 20364 || appId == 21000 || appId == 21044)
                    {
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Motorstorm US West",
                            Type = ChannelType.Lobby
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Motorstorm US Central",
                            Type = ChannelType.Lobby
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Motorstorm US East",
                            Type = ChannelType.Lobby
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Motorstorm EU",
                            Type = ChannelType.Lobby
                        });
                        /*
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "モーターストームJP",
                            Type = ChannelType.Lobby
                        });
                    }
                    #endregion

                    #region NBA 07
                    else if (appId == 20244)
                    {
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "SportsConnect US",
                            Type = ChannelType.Lobby,
                            GenericField1 = 1,
                            GenericField2 = 1,
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "SportsConnect EU",
                            Type = ChannelType.Lobby,
                            GenericField1 = 1,
                            GenericField2 = 1,
                        });
                    }
                    #endregion

                    #region WRC 4
                    else if (appId == 10394)
                    {
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Region US",
                            Type = ChannelType.Lobby,
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Region EU",
                            Type = ChannelType.Lobby,
                        });
                    }
                    #endregion

                    #region Socom 1
                    else if (appId == 10274)
                    {
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Region US",
                            Type = ChannelType.Lobby,
                        });
                    }
                    #endregion

                    #region Arc the Lad: End of Darkness
                    else if (appId == 10984)
                    {
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Yewbell",
                            Type = ChannelType.Lobby,
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Rueloon",
                            Type = ChannelType.Lobby,
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Dilzweld",
                            Type = ChannelType.Lobby,
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Milmarna",
                            Type = ChannelType.Lobby,
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Romastle Plains",
                            Type = ChannelType.Lobby,
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Halshinne",
                            Type = ChannelType.Lobby,
                        });
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Lamda Temple",
                            Type = ChannelType.Lobby,
                        });
                    }
                    #endregion

                    #region Default 
                    if (Manager.GetDefaultLobbyChannel(appId) == null)
                    {
                        Manager.AddChannel(new Channel()
                        {
                            ApplicationId = appId,
                            MaxPlayers = 256,
                            Name = "Default",
                            Type = ChannelType.Lobby
                        });
                    }
                    #endregion
                }
            }
            else
            {
                if (Manager.GetDefaultLobbyChannel(0) == null)
                {
                    Manager.AddChannel(new Channel()
                    {
                        ApplicationId = 0,
                        MaxPlayers = 256,
                        Name = "Default",
                        Type = ChannelType.Lobby
                    });
                }
            }
            */

        }

        /// <summary>
        /// 
        /// </summary>
        static Task RefreshConfig()
        {
            var usePublicIp = Settings.UsePublicIp;

            // 
            var serializerSettings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            #region Dirs
            string root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string subdirLogs = root + @"\logs";
            string subdirHorizonPlugins = root + @"\plugins";
            string subdirConfig = root + @"\config";
            string subdirConfigFile = subdirConfig + @"\" + CONFIG_FILE;
            #endregion

            /*
            #region Create Dirs
            // If Logs directory does not exist, create it. 
            if (!Directory.Exists(subdirLogs))
            {
                Directory.CreateDirectory(subdirLogs);
            }

            // If config directory does not exist, create it. 
            if (!Directory.Exists(subdirConfig))
            {
                Directory.CreateDirectory(subdirConfig);
            }
            #endregion
            */

            #region Check Config.json
            // Create Defaults if File doesn't exist
            if (!File.Exists(CONFIG_FILE))
            {
                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
            else
            {
                // Populate existing object
                JsonConvert.PopulateObject(File.ReadAllText(CONFIG_FILE), Settings, serializerSettings);
            }
            #endregion

            // Determine server ip
            RefreshServerIp();

            // Update NAT Ip with server ip if null
            if (string.IsNullOrEmpty(Settings.NATIp))
                Settings.NATIp = SERVER_IP.ToString();

            // Update default rsa key
            Server.Pipeline.Attribute.ScertClientAttribute.DefaultRsaAuthKey = Settings.DefaultKey;

            if (Settings.DefaultKey != null)
                GlobalAuthPublic = new RSA_KEY(Settings.DefaultKey.N.ToByteArrayUnsigned().Reverse().ToArray());

            //
            _ = RefreshAppSettings();

            // Load tick time into sleep ms for main loop
            sleepMS = TickMS;
            return Task.CompletedTask;
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

                // get locations
                var locations = await Database.GetLocations();
                var channels = await Database.GetChannels();

                // add new channels
                foreach (var channel in channels)
                {
                    if (Manager.GetChannelByChannelId(channel.Id, channel.AppId) == null)
                    {
                        await Manager.AddChannel(new Channel()
                        {
                            Id = channel.Id,
                            Name = channel.Name,
                            ApplicationId = channel.AppId,
                            MaxPlayers = channel.MaxPlayers,
                            GenericField1 = channel.GenericField1,
                            GenericField2 = channel.GenericField2,
                            GenericField3 = channel.GenericField3,
                            GenericField4 = channel.GenericField4,
                            GenericFieldLevel = (MediusWorldGenericFieldLevelType)channel.GenericFieldFilter,
                            Type = ChannelType.Lobby
                        });
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
            #region Determine Server IP
            if (!Settings.UsePublicIp)
            {
                SERVER_IP = Utils.GetLocalIPAddress();
                IP_TYPE = "Local";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Settings.PublicIpOverride))
                {
                    SERVER_IP = IPAddress.Parse(Utils.GetPublicIPAddress());
                    IP_TYPE = "Public";

                }
                else
                {
                    SERVER_IP = IPAddress.Parse(Settings.PublicIpOverride);
                    IP_TYPE = "Public (Override)";
                }
            }
            #endregion
        }

        /// <summary>
        /// Generates a incremental session key number
        /// </summary>
        /// <returns></returns>
        public static string GenerateSessionKey()
        {
            lock (_sessionKeyCounterLock)
            {
                return (++_sessionKeyCounter).ToString();
            }
        }

        #region Text Filter
        private static string GetTextFilterRegexExpression(int appId, TextFilterContext context)
        {
            var appSettings = GetAppSettingsOrDefault(appId);
            string regex = null;

            switch (context)
            {
                case TextFilterContext.ACCOUNT_NAME: regex = appSettings.TextFilterAccountName; break;
                case TextFilterContext.CHAT: regex = appSettings.TextFilterChat; break;
                case TextFilterContext.CLAN_MESSAGE: regex = appSettings.TextFilterClanMessage; break;
                case TextFilterContext.CLAN_NAME: regex = appSettings.TextFilterClanName; break;
                case TextFilterContext.DEFAULT: regex = appSettings.TextFilterDefault; break;
                case TextFilterContext.GAME_NAME: regex = appSettings.TextFilterGameName; break;
            }

            if (string.IsNullOrEmpty(regex))
                return appSettings.TextFilterDefault;

            return regex;
        }

        public static bool PassTextFilter(int appId, TextFilterContext context, string text)
        {
            var rExp = GetTextFilterRegexExpression(appId, context);
            if (string.IsNullOrEmpty(rExp))
                return true;

            Regex r = new Regex(rExp, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return !r.IsMatch(text);
        }

        public static string FilterTextFilter(int appId, TextFilterContext context, string text)
        {
            var rExp = GetTextFilterRegexExpression(appId, context);
            if (string.IsNullOrEmpty(rExp))
                return text;

            Regex r = new Regex(rExp, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return r.Replace(text, "");
        }
        #endregion

        public static string GetFileSystemPath(int appId, string filename)
        {
            if (!GetAppSettingsOrDefault(appId).EnableMediusFileServices)
                return null;
            if (string.IsNullOrEmpty(Settings.MediusFileServerRootPath))
                return null;
            if (string.IsNullOrEmpty(filename))
                return null;

            var rootPath = Path.GetFullPath(Settings.MediusFileServerRootPath);
            var path = Path.GetFullPath(Path.Combine(Settings.MediusFileServerRootPath, appId.ToString(), filename));

            // prevent filename from moving up directories
            if (!path.StartsWith(rootPath))
                return null;

            return path;
        }

        /// <summary>
        /// Gets File Path with AppId included in the path
        /// </summary>
        /// <param name="appId">AppId passed in</param>
        /// <returns></returns>
        public static string GetFileAppIdPath(int appId)
        {
            if (!GetAppSettingsOrDefault(appId).EnableMediusFileServices)
                return null;
            if (string.IsNullOrEmpty(Settings.MediusFileServerRootPath))
                return null;

            var rootPath = Path.GetFullPath(Settings.MediusFileServerRootPath);
            var path = Path.Combine(rootPath, appId.ToString());

            if (!Directory.Exists(path))
            {
                Logger.Warn($"Path being created! {path} for appId {appId}");
                Directory.CreateDirectory(path);
            }

            // prevent filename from moving up directories
            if (!path.StartsWith(rootPath))
                return null;

            return path;
        }

        public static AppSettings GetAppSettingsOrDefault(int appId)
        {
            if (_appSettings.TryGetValue(appId, out var appSettings))
                return appSettings;

            return _defaultAppSettings;
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
            ManagementObject mo = new ManagementObject(@"\\.\root\cimv2:Win32_OperatingSystem=@");
            DateTime lastBootUp = ManagementDateTimeConverter.ToDateTime(mo["LastBootUpTime"].ToString());
            return DateTime.Now.ToUniversalTime() - lastBootUp.ToUniversalTime();
        }
        #endregion

        #region DoGetHost
        public static void DoGetHostNameEntry(string hostName)
        {
            IPHostEntry host = Dns.GetHostEntry(hostName);
            try
            {
                Logger.Info($"NAT Service HostName: {hostName} \n      NAT Service IP: {host.AddressList.First()}");
                //Logger.Info($"GetHostEntry({address}) returns HostName: {host.HostName}");
            }
            catch (SocketException ex)
            {
                //unknown host or
                //not every IP has a name
                //log exception (manage it)
                Logger.Error($"Unable to resolve NAT service IP: {host.AddressList.First()}  Exiting with exception: {ex}");
            }
        }

        public static void DoGetHostAddressEntry(IPAddress address)
        {
            IPHostEntry host = Dns.GetHostEntry(address);
            try
            {
                Logger.Info($"NAT Service IP: {host.AddressList.First()}");
                //Logger.Info($"GetHostEntry({address}) returns HostName: {host.HostName}");
            }
            catch (SocketException ex)
            {
                //unknown host or
                //not every IP has a name
                //log exception (manage it)
                Logger.Info($"Unable to resolve NAT service IP: {host.AddressList.First()}  Exiting with exception: {ex}");
            }
        }
        #endregion

        public static void MFS_transferInit()
        {
            Logger.Info($"Initializing MFS_transfer with url {Settings.MFSTransferURI} "); //numThreads{}"

        }

        public static void DMEServerResetMetrics()
        {
            //rt_msg_server_reset_connect_metrics
            //rt_msg_server_reset_data_metrics
            //rt_msg_server_reset_message_metrics
            //rt_msg_server_reset_frame_time_metrics

            //ERROR: Could not reset DME Svr metrics[%d]
        }

        public static Medius.MPS GetMPS()
        {
            Medius.MPS mps = ProxyServer;

            return mps;
        }
    }
}
