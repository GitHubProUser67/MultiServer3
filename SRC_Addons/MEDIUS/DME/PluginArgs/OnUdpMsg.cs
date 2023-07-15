using PSMultiServer.SRC_Addons.MEDIUS.DME.Models;
using PSMultiServer.SRC_Addons.MEDIUS.Server.Pipeline.Udp;

namespace PSMultiServer.SRC_Addons.MEDIUS.DME.PluginArgs
{
    public class OnUdpMsg
    {
        public ClientObject Player { get; set; }

        public ScertDatagramPacket Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
