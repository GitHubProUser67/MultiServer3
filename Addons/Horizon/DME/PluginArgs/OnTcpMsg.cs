using MultiServer.Addons.Horizon.RT.Models;
using MultiServer.Addons.Horizon.DME.Models;

namespace MultiServer.Addons.Horizon.DME.PluginArgs
{
    public class OnTcpMsg
    {
        public ClientObject Player { get; set; }

        public BaseScertMessage Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
