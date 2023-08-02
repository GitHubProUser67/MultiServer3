using PSMultiServer.Addons.Horizon.RT.Models;
using PSMultiServer.Addons.Horizon.DME.Models;

namespace PSMultiServer.Addons.Horizon.DME.PluginArgs
{
    public class OnTcpMsg
    {
        public ClientObject Player { get; set; }

        public BaseScertMessage Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
