using PSMultiServer.Addons.Horizon.DME.Models;
using PSMultiServer.Addons.Horizon.Server.Pipeline.Udp;

namespace PSMultiServer.Addons.Horizon.DME.PluginArgs
{
    public class OnUdpMsg
    {
        public ClientObject Player { get; set; }

        public ScertDatagramPacket Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
