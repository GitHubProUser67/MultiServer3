using MultiServer.Addons.Horizon.DME.Models;
using MultiServer.Addons.Horizon.LIBRARY.Pipeline.Udp;

namespace MultiServer.Addons.Horizon.DME.PluginArgs
{
    public class OnUdpMsg
    {
        public ClientObject Player { get; set; }

        public ScertDatagramPacket Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
