using CustomLogger;
using Newtonsoft.Json;
using Horizon.BWPS.Config;
using Horizon.LIBRARY.Common;
using System.Diagnostics;
using Horizon.PluginManager;

namespace Horizon.BWPS
{
    public class BWPSClass
    {
        public static string? CONFIG_FILE = HorizonServerConfiguration.BWPSConfig;

        public static ServerSettings Settings = new();
        public static BWPS BWPS = new();

        public static MediusPluginsManager Plugins = new(HorizonServerConfiguration.PluginsFolder);

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
                LoggerAccessor.LogError(ex);
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

                LoggerAccessor.LogInfo("**************************************************");
                string datetime = DateTime.Now.ToString("MMMM/dd/yyyy hh:mm:ss tt");
                LoggerAccessor.LogInfo($"* Launched on {datetime}");

                Task.WaitAll(BWPS.Start());
                //string gpszVersion = "rt_bwprobe ReleaseVersion 3.02.200704101920";
                string gpszVersion2 = "3.02.200704101920";
                LoggerAccessor.LogInfo($"* Bandwidth Probe Server Version {gpszVersion2}");

                //* Process ID: %d , Parent Process ID: %d

                LoggerAccessor.LogInfo($"* Server Key Type: {Settings.EncryptMessages}");

                //* Diagnostic Profiling Enabled: %d Counts

                LoggerAccessor.LogInfo("**************************************************");

                LoggerAccessor.LogInfo($"UDP BWPS started on port {Settings.BWPSPort}.");

                started = true;

                _ = Task.Run(LoopServer);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[BWPS] - Server failed to initialize with error - {ex}");
            }

            return Task.CompletedTask;
        }

        public static void BWPSMain()
        {
            RefreshConfig();
            _ = StartServerAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        private static void RefreshConfig()
        {
            // Load settings
            if (File.Exists(CONFIG_FILE))
                // Populate existing object
                JsonConvert.PopulateObject(File.ReadAllText(CONFIG_FILE), Settings, new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                });
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_FILE) ?? Directory.GetCurrentDirectory() + "/static");

                // Save defaults
                File.WriteAllText(CONFIG_FILE ?? Directory.GetCurrentDirectory() + "/static/bwps.json", JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
        }
    }
}
