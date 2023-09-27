using Newtonsoft.Json;
using MultiServer.Addons.Horizon.NAT.Config;

namespace MultiServer.Addons.Horizon.NAT
{
    public class NATClass
    {
        public static string CONFIG_FILE = Directory.GetCurrentDirectory() + $"/{ServerConfiguration.NATConfig}";

        public static ServerSettings Settings = new ServerSettings();
        public static NAT NATServer = new NAT();

        public static bool started = false;

        private static async Task LoopServer()
        {
            while (started)
            {
                await NATServer.Tick();

                await Task.Delay(100);
            }
        }

        private static Task StartServerAsync()
        {
            try
            {
                ServerConfiguration.LogInfo($"Starting NAT on port {NATServer.Port}.");
                Task.WaitAll(NATServer.Start());
                ServerConfiguration.LogInfo($"NAT started.");

                started = true;

                _ = Task.Run(LoopServer);
            }
            catch (Exception ex)
            {
                ServerConfiguration.LogError($"[NAT] - Server failed to initialize with error - {ex}");
            }

            return Task.CompletedTask;
        }

        public static void NATMain()
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
