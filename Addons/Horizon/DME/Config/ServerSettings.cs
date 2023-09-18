using MultiServer.Addons.Org.BouncyCastle.Math;
using MultiServer.Addons.Horizon.RT.Cryptography.RSA;

namespace MultiServer.Addons.Horizon.DME.Config
{
    public class ServerSettings
    {
        /// <summary>
        /// MPS connection information
        /// </summary>
        public MPSSettings MPS { get; set; } = new();

        /// <summary>
        /// Key used to authenticate clients.
        /// </summary>
        public RsaKeyPair DefaultKey { get; set; } = new(
            new BigInteger("10315955513017997681600210131013411322695824559688299373570246338038100843097466504032586443986679280716603540690692615875074465586629501752500179100369237", 10),
            new BigInteger("17", 10),
            new BigInteger("4854567300243763614870687120476899445974505675147434999327174747312047455575182761195687859800492317495944895566174677168271650454805328075020357360662513", 10)
            );

        /// <summary>
        /// How many milliseconds before refreshing the config.
        /// </summary>
        public int RefreshConfigInterval = 5000;

        /// <summary>
        /// Application id.
        /// </summary>
        public List<int> ApplicationIds { get; set; } = new List<int>();

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
        /// Seconds between disconnects before the client attempts to reconnect to the proxy server.
        /// </summary>
        public int MPSReconnectInterval { get; set; } = 15;

        /// <summary>
        /// Milliseconds between plugin ticks.
        /// </summary>
        public int PluginTickIntervalMs { get; set; } = 50;

        /// <summary>
        /// Port of the TCP server.
        /// </summary>
        public int TCPPort { get; set; } = 10073;

        /// <summary>
        /// Port to bind the udp server to.
        /// </summary>
        public int UDPPort { get; set; } = 50000;

        /// <summary>
        /// This configuration setting enables the "auxilary udp" protocol. When enabled
        /// clients can optionally establish an unreliable udp channel in parallel to 
        /// their primary tcp channel.
        /// </summary>
        public bool EnableAuxUDP = true; // (DEFAULT: 0)
    }

    public class MPSSettings
    {
        /// <summary>
        /// Ip of the Medius Proxy Server.
        /// </summary>
        public string Ip { get; set; } = LIBRARY.Common.Utils.GetLocalIPAddress().ToString();

        /// <summary>
        /// The port that the Proxy Server is bound to.
        /// </summary>
        public int Port { get; set; } = 10077;

        /// <summary>
        /// Key used to establish initial handshake with MPS.
        /// </summary>
        public RsaKeyPair Key { get; set; } = new(
            new BigInteger("10315955513017997681600210131013411322695824559688299373570246338038100843097466504032586443986679280716603540690692615875074465586629501752500179100369237"),
            new BigInteger("17"),
            new BigInteger("4854567300243763614870687120476899445974505675147434999327174747312047455575182761195687859800492317495944895566174677168271650454805328075020357360662513")
            );
    }
}
