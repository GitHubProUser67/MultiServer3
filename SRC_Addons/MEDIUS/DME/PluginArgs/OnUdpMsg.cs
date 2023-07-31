using PSMultiServer.Addons.Medius.DME.Models;
using PSMultiServer.Addons.Medius.Server.Pipeline.Udp;

namespace PSMultiServer.Addons.Medius.DME.PluginArgs
{
    public class OnUdpMsg
    {
        public ClientObject Player { get; set; }

        public ScertDatagramPacket Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
