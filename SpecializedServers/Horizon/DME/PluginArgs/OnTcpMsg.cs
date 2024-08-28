using Horizon.RT.Models;
using Horizon.MUM.Models;

namespace Horizon.DME.PluginArgs
{
    public class OnTcpMsg
    {
        public ClientObject? Player { get; set; }

        public BaseScertMessage? Packet { get; set; }

        public bool Ignore { get; set; }
    }
}