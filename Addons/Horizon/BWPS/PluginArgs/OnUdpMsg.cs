using MultiServer.Addons.Horizon.LIBRARY.Pipeline.Udp;

namespace MultiServer.Addons.Horizon.BWPS.PluginArgs
{
    public class OnUdpMsg
    {
        public ScertDatagramPacket Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
