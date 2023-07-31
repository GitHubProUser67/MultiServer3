using PSMultiServer.Addons.Medius.RT.Models;
using PSMultiServer.Addons.Medius.DME.Models;

namespace PSMultiServer.Addons.Medius.DME.PluginArgs
{
    public class OnTcpMsg
    {
        public ClientObject Player { get; set; }

        public BaseScertMessage Packet { get; set; }

        public bool Ignore { get; set; }
    }
}
