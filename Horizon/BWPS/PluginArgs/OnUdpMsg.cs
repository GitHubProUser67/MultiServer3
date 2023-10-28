using Horizon.LIBRARY.Pipeline.Udp;

namespace Horizon.BWPS.PluginArgs
{
    public class OnUdpMsg
    {
        public ScertDatagramPacket Packet { get; set; }

        public bool Ignore { get; set; }
    }
}