using DotNetty.Common.Internal.Logging;
using Newtonsoft.Json;
using MultiServer.Addons.Horizon.BWPS.Config;
using MultiServer.Addons.Horizon.LIBRARY.Common;
using System.Diagnostics;

namespace MultiServer.Addons.Horizon.BWPS
{
    public class BWPSClass
    {
        public static string CONFIG_FILE = Directory.GetCurrentDirectory() + $"/{ServerConfiguration.BWPSConfig}";
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<Program>();

        public static ServerSettings Settings = new ServerSettings();
        public static BWPS BWPS = new BWPS();

        public static PluginsManager Plugins = null;

        public static readonly Stopwatch Stopwatch = Stopwatch.StartNew();

        private static DateTime _timeLastPluginTick = Utils.GetHighPrecisionUtcTime();

        private static DateTime _lastConfigRefresh = Utils.GetHighPrecisionUtcTime();

        public static bool started = false;

        private static async Task TickAsync()
        {
            try
            {
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
                ServerConfiguration.LogError(ex);
            }
        }

        private static async Task HandleInMessages()
        {
            // handle incoming
            {
                var tasks = new List<Task>()
                {
                    BWPS.HandleIncomingMessages()
                };

                await Task.WhenAll(tasks);
            }
        }

        private static async Task HandleOutMessages()
        {
            // handle outgoing
            {
                var tasks = new List<Task>()
                {
                    BWPS.HandleOutgoingMessages()
                };

                await Task.WhenAll(tasks);
            }
        }

        private static async Task LoopServer()
        {
            while (started)
            {
                await TickAsync();

                await Task.Delay(100);
            }

            await BWPS.Stop();
            await Task.WhenAll(BWPS.Stop());
        }

        private static Task StartServerAsync()
        {
            try
            {
                DateTime lastConfigRefresh = Utils.GetHighPrecisionUtcTime();

                ServerConfiguration.LogInfo("**************************************************");
                string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");
                ServerConfiguration.LogInfo($"* Launched on {datetime}");

                Task.WaitAll(BWPS.Start());
                //string gpszVersion = "rt_bwprobe ReleaseVersion 3.02.200704101920";
                string gpszVersion2 = "3.02.200704101920";
                ServerConfiguration.LogInfo($"* Bandwidth Probe Server Version {gpszVersion2}");

                //* Process ID: %d , Parent Process ID: %d

                ServerConfiguration.LogInfo($"* Server Key Type: {Settings.EncryptMessages}");

                //* Diagnostic Profiling Enabled: %d Counts

                ServerConfiguration.LogInfo("**************************************************");

                ServerConfiguration.LogInfo($"UDP BWPS started on port {Settings.BWPSPort}.");

                started = true;

                _ = Task.Run(LoopServer);
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[BWPS] - Server failed to initialize with error - {ex}");
            }

            return Task.CompletedTask;
        }

        public static void BWPSMain()
        {
            Initialize();
            // Initialize plugins
            Plugins = new PluginsManager(Server.pluginspath);
            _ = StartServerAsync();
        }

        private static void Initialize()
        {
            RefreshConfig();
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

            // Load settings
            if (File.Exists(CONFIG_FILE))
                // Populate existing object
                JsonConvert.PopulateObject(File.ReadAllText(CONFIG_FILE), Settings, serializerSettings);
            else
                // Save defaults
                File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(Settings, Formatting.Indented));
        }
    }
}
