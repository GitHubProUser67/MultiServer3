using Horizon.DME.Models;
using CryptoSporidium.Horizon.LIBRARY.Pipeline.Udp;

namespace Horizon.DME.PluginArgs
{
    public class OnUdpMsg
    {
        public ClientObject? Player { get; set; }

        public ScertDatagramPacket? Packet { get; set; }

        public bool Ignore { get; set; }
    }
}