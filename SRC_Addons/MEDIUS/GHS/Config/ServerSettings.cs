using Org.BouncyCastle.Math;
using PSMultiServer.SRC_Addons.MEDIUS.RT.Cryptography.RSA;

namespace PSMultiServer.SRC_Addons.MEDIUS.GHS.Config
{
    public class ServerSettings
    {
        /// <summary>
        /// How many milliseconds before refreshing the config.
        /// </summary>
        public int RefreshConfigInterval = 5000;

        /// <summary>
        /// Ports of the GHS.
        /// </summary>
        public int Port { get; set; } = 3101;

        #region NAT SCE-RT Service Location
        /// <summary>
        /// Ip address of the NAT server.
        /// Provide the IP of the SCE-RT NAT Service
        /// Default is: natservice.pdonline.scea.com:10070
        /// </summary>
        public string NATIp { get; set; } = Server.Common.Utils.GetLocalIPAddress().ToString();
        /// <summary>
        /// Port of the NAT server.
        /// Provide the Port of the SCE-RT NAT Service
        /// </summary>
        public int NATPort { get; set; } = 10070;
        #endregion

        #region Remote Log Viewer Port To Listen
        /// <summary>
        /// Any value greater than 0 will enable remote logging with the SCE-RT logviewer
        /// on that port, which must not be in use by other applications (default 0)
        /// </summary>
        public int RemoteLogViewPort = 0;
        #endregion

        /// <summary>
        /// Key used to authenticate clients.
        /// </summary>
        public RsaKeyPair DefaultKey { get; set; } = new RsaKeyPair(
            new BigInteger("10315955513017997681600210131013411322695824559688299373570246338038100843097466504032586443986679280716603540690692615875074465586629501752500179100369237", 10),
            new BigInteger("17", 10),
            new BigInteger("4854567300243763614870687120476899445974505675147434999327174747312047455575182761195687859800492317495944895566174677168271650454805328075020357360662513", 10)
            );

        /// <summary>
        /// Whether or not to encrypt messages.
        /// </summary>
        public bool EncryptMessages { get; set; } = true;
    }
}
