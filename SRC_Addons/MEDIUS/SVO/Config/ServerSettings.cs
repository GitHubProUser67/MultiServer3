using PSMultiServer.Addons.Medius.Server.Common.Logging;

namespace PSMultiServer.Addons.Medius.SVO.Config
{
    #region SVO Server Config
    public class ServerSettings
    {
        /// <summary>
        /// Path to the plugins directory.
        /// </summary>
        public string PluginsPath { get; set; } = "plugins/";

        /// <summary>
        /// Number of milliseconds for main loop thread to sleep.
        /// </summary>
        public int MainLoopSleepMs { get; set; } = 5;

        /// <summary>
        /// How many milliseconds before refreshing the config.
        /// </summary>
        public int RefreshConfigInterval = 5000;

        /// <summary>
        /// Number of ticks per second.
        /// </summary>
        public int TickRate { get; set; } = 10;

        #region PublicIp
        /// <summary>
        /// By default the server will grab its local ip.
        /// If this is set, it will use its public ip instead.
        /// </summary>
        public bool UsePublicIp { get; set; } = false;

        /// <summary>
        /// If UsePublicIp is set to true, allow overriding and skipping using dyndns's dynamic
        /// ip address finder, since it goes down often enough to throw exceptions
        /// </summary>
        public string PublicIpOverride { get; set; } = string.Empty;
        #endregion


        /// <summary>
        /// When set, all nat ip requests will be receive the server's ip and this port.
        /// </summary>
        public int? OverridePort { get; set; } = null;
    }
    #endregion
}
