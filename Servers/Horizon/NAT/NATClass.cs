using CustomLogger;
using Newtonsoft.Json;
using Horizon.NAT.Config;

namespace Horizon.NAT
{
    public class NATClass
    {
        public static string? CONFIG_FILE = HorizonServerConfiguration.NATConfig;

        public static ServerSettings Settings = new();
        public static NAT NATServer = new();

        public static bool started = false;

        private static async Task LoopServer()
        {
            while (started)
            {
                await NATServer.Tick();

                await Task.Delay(100);
            }
        }

        public static async void StopServer()
        {
            started = false;

            await NATServer.Stop();
        }

        private static Task StartServerAsync()
        {
            try
            {
                LoggerAccessor.LogInfo($"Starting NAT on port {NATServer.Port}.");
                Task.WaitAll(NATServer.Start());
                LoggerAccessor.LogInfo($"NAT started.");

                started = true;

                _ = Task.Run(LoopServer);
            }
            catch (Exception ex)
            {
                LoggerAccessor.LogError($"[NAT] - Server failed to initialize with error - {ex}");
            }

            return Task.CompletedTask;
        }

        public static void StartServer()
        {
            Initialize();
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
                File.WriteAllText(CONFIG_FILE ?? Directory.GetCurrentDirectory() + "/static/nat.json", JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
        }
    }
}
