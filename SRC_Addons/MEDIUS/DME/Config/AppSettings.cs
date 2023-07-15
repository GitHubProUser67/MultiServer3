namespace PSMultiServer.SRC_Addons.MEDIUS.DME.Config
{
    public class AppSettings
    {
        /// <summary>
        /// This settings respective app id.
        /// </summary>
        public int AppId { get; }

        /// <summary>
        /// When true, server will encrypt all messages.
        /// </summary>
        public bool EnableDmeEncryption { get; private set; } = false;

        /// <summary>
        /// Default time in milliseconds for the client's world agg time.
        /// </summary>
        public int DefaultClientWorldAggTime { get; private set; } = 20;

        /// <summary>
        /// Number of seconds before the server should send an echo to the client.
        /// </summary>
        public int ServerEchoIntervalSeconds { get; private set; } = 5;

        /// <summary>
        /// Period of time when a client is moving between medius server components where the client object will be kept alive.
        /// </summary>
        public int KeepAliveGracePeriodSeconds { get; private set; } = 8;

        /// <summary>
        /// Time since last echo before timing the client out.
        /// </summary>
        public int ClientTimeoutSeconds { get; private set; } = 25;

        /// <summary>
        /// Time since game created and host never connected to close the game world.
        /// </summary>
        public int GameTimeoutSeconds { get; private set; } = 15;

        public AppSettings(int appId)
        {
            AppId = appId;
        }

        public void SetSettings(Dictionary<string, string> settings)
        {
            string value = null;

            // EnableDmeEncryption
            if (settings.TryGetValue("EnableDmeEncryption", out value) && bool.TryParse(value, out var enableDmeEncryption))
                EnableDmeEncryption = enableDmeEncryption;
            // DefaultClientWorldAggTime
            if (settings.TryGetValue("DefaultClientWorldAggTime", out value) && int.TryParse(value, out var defaultClientWorldAggTime))
                DefaultClientWorldAggTime = defaultClientWorldAggTime;
            // ServerEchoIntervalSeconds
            if (settings.TryGetValue("ServerEchoIntervalSeconds", out value) && int.TryParse(value, out var serverEchoIntervalSeconds))
                ServerEchoIntervalSeconds = serverEchoIntervalSeconds;
            // KeepAliveGracePeriodSeconds
            if (settings.TryGetValue("KeepAliveGracePeriodSeconds", out value) && int.TryParse(value, out var keepAliveGracePeriodSeconds))
                KeepAliveGracePeriodSeconds = keepAliveGracePeriodSeconds;
            // ClientTimeoutSeconds
            if (settings.TryGetValue("ClientTimeoutSeconds", out value) && int.TryParse(value, out var clientTimeoutSeconds))
                ClientTimeoutSeconds = clientTimeoutSeconds;
            // GameTimeoutSeconds
            if (settings.TryGetValue("GameTimeoutSeconds", out value) && int.TryParse(value, out var gameTimeoutSeconds))
                GameTimeoutSeconds = gameTimeoutSeconds;
        }

        public Dictionary<string, string> GetSettings()
        {
            return new Dictionary<string, string>()
            {
                { "EnableDmeEncryption", EnableDmeEncryption.ToString() },
                { "DefaultClientWorldAggTime", DefaultClientWorldAggTime.ToString() },
                { "ServerEchoIntervalSeconds", ServerEchoIntervalSeconds.ToString() },
                { "KeepAliveGracePeriodSeconds", KeepAliveGracePeriodSeconds.ToString() },
                { "ClientTimeoutSeconds", ClientTimeoutSeconds.ToString() },
                { "GameTimeoutSeconds", GameTimeoutSeconds.ToString() },
            };
        }
    }
}
